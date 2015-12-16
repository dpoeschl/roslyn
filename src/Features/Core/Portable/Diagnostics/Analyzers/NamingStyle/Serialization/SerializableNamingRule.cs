using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
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
                info.GetNamingStyle(NamingStyleID));
        }
    }
}
