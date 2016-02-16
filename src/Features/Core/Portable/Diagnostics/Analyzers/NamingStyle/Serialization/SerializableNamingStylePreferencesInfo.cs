// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
{
    /// <summary>
    /// Contains all information related to Naming Style Preferences.
    /// 1. Symbol Specifications
    /// 2. Name Style
    /// 3. Naming Rule (points to Symbol Specification IDs)
    /// </summary>
    [DataContract]
    internal class SerializableNamingStylePreferencesInfo
    {
        [DataMember]
        public List<SymbolSpecification> SymbolSpecifications;
        [DataMember]
        public List<NamingStyle> NamingStyles;
        [DataMember]
        public List<SerializableNamingRule> NamingRules;

        internal SerializableNamingStylePreferencesInfo()
        {
            SymbolSpecifications = new List<SymbolSpecification>();
            NamingStyles = new List<NamingStyle>();
            NamingRules = new List<SerializableNamingRule>();
        }

        internal NamingStyle GetNamingStyle(Guid namingStyleID)
        {
            return NamingStyles.Single(s => s.ID == namingStyleID);
        }

        internal SymbolSpecification GetSymbolSpecification(Guid symbolSpecificationID)
        {
            return SymbolSpecifications.Single(s => s.ID == symbolSpecificationID);
        }

        public NamingStylePreferencesInfo GetPreferencesInfo()
        {
            var info = new NamingStylePreferencesInfo();
            info.NamingRules = NamingRules.Select(r => r.GetRule(this)).ToList();
            return info;
        }
    }
}
