﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.LanguageServices.Implementation.LanguageService;
using Microsoft.VisualStudio.RpcContracts.OutputChannel;
using Microsoft.VisualStudio.RpcContracts.Settings;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options
{
    /// <summary>
    /// Serializes settings marked with <see cref="RoamingProfileStorageLocation"/> to and from the user's roaming profile.
    /// </summary>
    [Export(typeof(IOptionPersister))]
    internal sealed class RoamingVisualStudioProfileOptionPersister : ForegroundThreadAffinitizedObject, IOptionPersister
    {
        // NOTE: This service is not public or intended for use by teams/individuals outside of Microsoft. Any data stored is subject to deletion without warning.
        [Guid("9B164E40-C3A2-4363-9BC5-EB4039DEF653")]
        private class SVsSettingsPersistenceManager { };

        private IBrokeredSettingsManager _brokeredSettingsManager;

        private readonly IGlobalOptionService _globalOptionService;

        /// <summary>
        /// The list of options that have been been fetched from <see cref="_brokeredSettingsManager"/>, by key. We track this so
        /// if a later change happens, we know to refresh that value. This is synchronized with monitor locks on
        /// <see cref="_optionsToMonitorForChangesGate" />.
        /// </summary>
        private readonly Dictionary<string, List<OptionKey>> _optionsToMonitorForChanges = new Dictionary<string, List<OptionKey>>();
        private readonly object _optionsToMonitorForChangesGate = new object();
        private IServiceBroker _serviceBroker;

        /// <remarks>We make sure this code is from the UI by asking for all serializers on the UI thread in <see cref="HACK_AbstractCreateServicesOnUiThread"/>.</remarks>
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public RoamingVisualStudioProfileOptionPersister(IThreadingContext threadingContext, IGlobalOptionService globalOptionService, [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(threadingContext, assertIsForeground: true) // The GetService call requires being on the UI thread or else it will marshal and risk deadlock
        {
            Contract.ThrowIfNull(globalOptionService);

            _globalOptionService = globalOptionService;

            var serviceContainer = serviceProvider.GetService<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
            _serviceBroker = serviceContainer.GetFullAccessServiceBroker();
            _serviceBroker.AvailabilityChanged += Sb_AvailabilityChanged;

            ConnectSettingsManager();
        }

        private void ConnectSettingsManager()
        {
            ThreadingContext.JoinableTaskFactory.Run(
                async () =>
                {
                    _brokeredSettingsManager = await _serviceBroker.GetProxyAsync<IBrokeredSettingsManager>(VisualStudioServices.VS2019_4.SettingsManager);
                    await _brokeredSettingsManager.RequestChangeEventsAsync("*", CancellationToken.None).ConfigureAwait(true);
                    _brokeredSettingsManager.SettingChanged += OnSettingChanged;
                    return;
                });
        }

        private void Sb_AvailabilityChanged(object sender, ServiceHub.Framework.BrokeredServicesChangedEventArgs e)
        {
            ConnectSettingsManager();
        }

        private void OnSettingChanged(object sender, PropertyChangedEventArgs args)
        {
            List<OptionKey> optionsToRefresh = null;

            lock (_optionsToMonitorForChangesGate)
            {
                if (_optionsToMonitorForChanges.TryGetValue(args.PropertyName, out var optionsToRefreshInsideLock))
                {
                    // Make a copy of the list so we aren't using something that might mutate underneath us.
                    optionsToRefresh = optionsToRefreshInsideLock.ToList();
                }
            }

            if (optionsToRefresh != null)
            {
                // Refresh the actual options outside of our _optionsToMonitorForChangesGate so we avoid any deadlocks by calling back
                // into the global option service under our lock. There isn't some race here where if we were fetching an option for the first time
                // while the setting was changed we might not refresh it. Why? We call RecordObservedValueToWatchForChanges before we fetch the value
                // and since this event is raised after the setting is modified, any new setting would have already been observed in GetFirstOrDefaultValue.
                // And if it wasn't, this event will then refresh it.
                foreach (var optionToRefresh in optionsToRefresh)
                {
                    if (TryFetch(optionToRefresh, out var optionValue))
                    {
                        _globalOptionService.RefreshOption(optionToRefresh, optionValue);
                    }
                }
            }
        }

        private object GetFirstOrDefaultValue(OptionKey optionKey, IEnumerable<RoamingProfileStorageLocation> roamingSerializations)
        {
            // There can be more than 1 roaming location in the order of their priority.
            // When fetching a value, we iterate all of them until we find the first one that exists.
            // When persisting a value, we always use the first location.
            // This functionality exists for breaking changes to persistence of some options. In such a case, there
            // will be a new location added to the beginning with a new name. When fetching a value, we might find the old
            // location (and can upgrade the value accordingly) but we only write to the new location so that
            // we don't interfere with older versions. This will essentially "fork" the user's options at the time of upgrade.

            return ThreadingContext.JoinableTaskFactory.Run<object>(
                async () =>
                {
                    foreach (var roamingSerialization in roamingSerializations)
                    {
                        var storageKey = roamingSerialization.GetKeyNameForLanguage(optionKey.Language);

                        RecordObservedValueToWatchForChanges(optionKey, storageKey);

                        var getResult = await _brokeredSettingsManager.GetValueAsync(storageKey, CancellationToken.None);

                        if (getResult.Succeeded)
                        {
                            return ((JValue)getResult.Value).Value;
                        }
                    }

                    return optionKey.Option.DefaultValue;
                });
        }

        public bool TryFetch(OptionKey optionKey, out object value)
        {
            if (_brokeredSettingsManager == null)
            {
                Debug.Fail("Manager field is unexpectedly null.");
                value = null;
                return false;
            }

            // Do we roam this at all?
            var roamingSerializations = optionKey.Option.StorageLocations.OfType<RoamingProfileStorageLocation>();

            if (!roamingSerializations.Any())
            {
                value = null;
                return false;
            }

            value = GetFirstOrDefaultValue(optionKey, roamingSerializations);

            // VS's ISettingsManager has some quirks around storing enums.  Specifically,
            // it *can* persist and retrieve enums, but only if you properly call 
            // GetValueOrDefault<EnumType>.  This is because it actually stores enums just
            // as ints and depends on the type parameter passed in to convert the integral
            // value back to an enum value.  Unfortunately, we call GetValueOrDefault<object>
            // and so we get the value back as boxed integer.
            //
            // Because of that, manually convert the integer to an enum here so we don't
            // crash later trying to cast a boxed integer to an enum value.
            if (optionKey.Option.Type.IsEnum)
            {
                if (value != null)
                {
                    value = Enum.ToObject(optionKey.Option.Type, value);
                }
            }
            else if (typeof(ICodeStyleOption).IsAssignableFrom(optionKey.Option.Type))
            {
                return DeserializeCodeStyleOption(ref value, optionKey.Option.Type);
            }
            else if (optionKey.Option.Type == typeof(NamingStylePreferences))
            {
                // We store these as strings, so deserialize
                if (value is string serializedValue)
                {
                    try
                    {
                        value = NamingStylePreferences.FromXElement(XElement.Parse(serializedValue));
                    }
                    catch (Exception)
                    {
                        value = null;
                        return false;
                    }
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            else if (optionKey.Option.Type == typeof(bool) && value is int intValue)
            {
                // TypeScript used to store some booleans as integers. We now handle them properly for legacy sync scenarios.
                value = intValue != 0;
                return true;
            }
            else if (optionKey.Option.Type == typeof(bool) && value is long longValue)
            {
                // TypeScript used to store some booleans as integers. We now handle them properly for legacy sync scenarios.
                value = longValue != 0;
                return true;
            }
            else if (optionKey.Option.Type == typeof(bool?))
            {
                // code uses object to hold onto any value which will use boxing on value types.
                // see boxing on nullable types - https://msdn.microsoft.com/en-us/library/ms228597.aspx
                return (value is bool) || (value == null);
            }
            else if (value != null && optionKey.Option.Type != value.GetType())
            {
                // We got something back different than we expected, so fail to deserialize
                value = null;
                return false;
            }

            return true;
        }

        private bool DeserializeCodeStyleOption(ref object value, Type type)
        {
            if (value is string serializedValue)
            {
                try
                {
                    var fromXElement = type.GetMethod(nameof(CodeStyleOption<object>.FromXElement), BindingFlags.Public | BindingFlags.Static);

                    value = fromXElement.Invoke(null, new object[] { XElement.Parse(serializedValue) });
                    return true;
                }
                catch (Exception)
                {
                }
            }

            value = null;
            return false;
        }

        private void RecordObservedValueToWatchForChanges(OptionKey optionKey, string storageKey)
        {
            // We're about to fetch the value, so make sure that if it changes we'll know about it
            lock (_optionsToMonitorForChangesGate)
            {
                var optionKeysToMonitor = _optionsToMonitorForChanges.GetOrAdd(storageKey, _ => new List<OptionKey>());

                if (!optionKeysToMonitor.Contains(optionKey))
                {
                    optionKeysToMonitor.Add(optionKey);
                }
            }
        }

        public bool TryPersist(OptionKey optionKey, object value)
        {
            if (_brokeredSettingsManager == null)
            {
                Debug.Fail("Manager field is unexpectedly null.");
                return false;
            }

            // Do we roam this at all?
            var roamingSerialization = optionKey.Option.StorageLocations.OfType<RoamingProfileStorageLocation>().FirstOrDefault();

            if (roamingSerialization == null)
            {
                value = null;
                return false;
            }

            var storageKey = roamingSerialization.GetKeyNameForLanguage(optionKey.Language);

            RecordObservedValueToWatchForChanges(optionKey, storageKey);

            if (value is ICodeStyleOption codeStyleOption)
            {
                // We store these as strings, so serialize
                value = codeStyleOption.ToXElement().ToString();
            }
            else if (optionKey.Option.Type == typeof(NamingStylePreferences))
            {
                // We store these as strings, so serialize
                var valueToSerialize = value as NamingStylePreferences;

                if (value != null)
                {
                    value = valueToSerialize.CreateXElement().ToString();
                }
            }

            _brokeredSettingsManager.SetValueAsync(storageKey, value, CancellationToken.None);
            return true;
        }
    }
}
