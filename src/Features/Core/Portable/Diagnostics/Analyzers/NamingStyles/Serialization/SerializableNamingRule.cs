// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles
{
    [DataContract]
    internal class SerializableNamingRule
    {
        [DataMember]
        public string Title;
        [DataMember]
        public List<SerializableNamingRule> Children;
        [DataMember]
        public Guid SymbolSpecificationID;
        [DataMember]
        public Guid NamingStyleID;
        [DataMember]
        public DiagnosticSeverity EnforcementLevel;

        internal SerializableNamingRule()
        {
            Children = new List<SerializableNamingRule>();
        }

        public NamingRule GetRule(SerializableNamingStylePreferencesInfo info)
        {
            return new NamingRule(
                Title,
                Children.Select(c => c.GetRule(info)).ToList(),
                info.GetSymbolSpecification(SymbolSpecificationID),
                info.GetNamingStyle(NamingStyleID),
                EnforcementLevel);
        }
    }
}
