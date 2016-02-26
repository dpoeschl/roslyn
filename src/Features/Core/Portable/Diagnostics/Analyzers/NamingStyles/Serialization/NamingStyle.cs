// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Roslyn.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles
{
    [DataContract]
    internal class NamingStyle : INamingStyle
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Prefix { get; set; }
        [DataMember]
        public string Suffix { get; set; }
        [DataMember]
        public string WordSeparator { get; set; }
        [DataMember]
        public Capitalization CapitalizationScheme { get; set; }

        public IEnumerable<string> InferWords(string name)
        {
            // TODO
            return SpecializedCollections.SingletonEnumerable(name);
        }

        public string CreateName(IEnumerable<string> words)
        {
            EnsureNonNullProperties();

            var wordsWithCasing = ApplyCapitalization(words);
            var combinedWordsWithCasing = string.Join(WordSeparator, wordsWithCasing);
            return Prefix + combinedWordsWithCasing + Suffix;
        }

        private IEnumerable<string> ApplyCapitalization(IEnumerable<string> words)
        {
            switch (CapitalizationScheme)
            {
                case Capitalization.PascalCase:
                    return words.Select(w => CapitalizeFirstLetter(w));
                case Capitalization.CamelCase:
                    return words.Take(1).Select(w => DecapitalizeFirstLetter(w)).Concat(words.Skip(1).Select(w => CapitalizeFirstLetter(w)));
                case Capitalization.FirstUpper:
                    return words.Take(1).Select(w => CapitalizeFirstLetter(w)).Concat(words.Skip(1).Select(w => DecapitalizeFirstLetter(w)));
                case Capitalization.AllUpper:
                    return words.Select(w => w.ToUpper());
                case Capitalization.AllLower:
                    return words.Select(w => w.ToLower());
                default:
                    throw new InvalidOperationException();
            }
        }

        private string CapitalizeFirstLetter(string word)
        {
            var chars = word.ToCharArray();
            return new string(chars.Take(1).Select(c => char.ToUpper(c)).Concat(chars.Skip(1)).ToArray());
        }

        private string DecapitalizeFirstLetter(string word)
        {
            var chars = word.ToCharArray();
            return new string(chars.Take(1).Select(c => char.ToLower(c)).Concat(chars.Skip(1)).ToArray());
        }

        public bool IsNameCompliant(string name, out string failureReason)
        {
            EnsureNonNullProperties();

            if (!name.StartsWith(Prefix))
            {
                failureReason = string.Format(FeaturesResources.NamingViolation_MissingPrefix, Prefix);
                return false;
            }

            if (!name.EndsWith(Suffix))
            {
                failureReason = string.Format(FeaturesResources.NamingViolation_MissingSuffix, Suffix);
                return false;
            }

            if (name.Length <= Prefix.Length + Suffix.Length)
            {
                failureReason = string.Empty;
                return true;
            }

            name = name.Substring(Prefix.Length);
            name = name.Substring(0, name.Length - Suffix.Length);

            var words = new[] { name };
            if (!string.IsNullOrEmpty(WordSeparator))
            {
                words = name.Split(new[] { WordSeparator }, StringSplitOptions.RemoveEmptyEntries);
            }

            failureReason = string.Empty;
            switch (CapitalizationScheme)
            {
                case Capitalization.PascalCase:
                    if (words.All(w => char.IsUpper(w[0])))
                    {
                        return true;
                    }
                    else
                    {
                        var violations = words.All(w => !char.IsUpper(w[0]));
                        failureReason = string.Format(FeaturesResources.NamingViolation_WordsMustBeginWithUppercase, string.Join(", ", violations));
                        return false;
                    }
                case Capitalization.CamelCase:
                    if (char.IsLower(words.First()[0]) && words.Skip(1).All(w => char.IsUpper(w[0])))
                    {
                        return true;
                    }
                    else
                    {
                        if (!char.IsLower(words.First()[0]))
                        {
                            failureReason = FeaturesResources.NamingViolation_FirstWordMustBeginWithLowercase;
                        }

                        var violations = words.Skip(1).Where(w => !char.IsUpper(w[0]));
                        if (violations.Any())
                        {
                            if (failureReason != string.Empty)
                            {
                                failureReason += Environment.NewLine;
                            }

                            failureReason += string.Format(FeaturesResources.NamingViolation_NonLeadingWordsMustBeginWithUppercase, string.Join(", ", violations));
                        }

                        return false;
                    }
                case Capitalization.FirstUpper:
                    if (char.IsUpper(words.First()[0]) && words.Skip(1).All(w => char.IsLower(w[0])))
                    {
                        return true;
                    }
                    else
                    {
                        if (!char.IsUpper(words.First()[0]))
                        {
                            failureReason = string.Format(FeaturesResources.NamingViolation_FirstWordMustBeginWithUppercase, words.First());
                        }

                        var violations = words.Skip(1).Where(w => !char.IsLower(w[0]));
                        if (violations.Any())
                        {
                            if (failureReason != string.Empty)
                            {
                                failureReason += Environment.NewLine;
                            }

                            failureReason += string.Format(FeaturesResources.NamingViolation_NonLeadingWordsMustBeginWithLowercase, string.Join(", ", violations));
                        }

                        return false;
                    }
                case Capitalization.AllUpper:
                    if (words.SelectMany(w => w.ToCharArray()).All(c => char.IsUpper(c)))
                    {
                        return true;
                    }
                    else
                    {
                        var violations = words.Where(w => !w.ToCharArray().All(c => char.IsUpper(c)));
                        failureReason = string.Format(FeaturesResources.NamingViolation_WordsCannotContainLowercaseLetters, string.Join(", ", violations));
                        return false;
                    }
                case Capitalization.AllLower:
                    if (words.SelectMany(w => w.ToCharArray()).All(c => char.IsLower(c)))
                    {
                        return true;
                    }
                    else
                    {
                        var violations = words.Where(w => !w.ToCharArray().All(c => char.IsLower(c)));
                        failureReason = string.Format(FeaturesResources.NamingViolation_WordsCannotContainUppercaseLetters, string.Join(", ", violations));
                        return false;
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        private string CreateCompliantNameDirectly(string name)
        {
            EnsureNonNullProperties();

            bool addPrefix = !name.StartsWith(Prefix);
            bool addSuffix = !name.EndsWith(Suffix);

            name = addPrefix ? (Prefix + name) : name;
            name = addSuffix ? (name + Suffix) : name;

            return FinishFixingName(name);
        }

        public IEnumerable<string> MakeCompliant(string name)
        {
            var name1 = CreateCompliantNameReusingPartialPrefixesAndSuffixes(name);
            yield return name1;

            var name2 = CreateCompliantNameDirectly(name);
            if (name2 != name1)
            {
                yield return name2;
            }
        }

        private string CreateCompliantNameReusingPartialPrefixesAndSuffixes(string name)
        {
            EnsureNonNullProperties();

            name = EnsurePrefix(name);
            name = EnsureSuffix(name);

            return FinishFixingName(name);
        }

        private string FinishFixingName(string name)
        {
            // Edge case: prefix "as", suffix "sa", name "asa"
            if (Suffix.Length + Prefix.Length >= name.Length)
            {
                return name;
            }

            name = name.Substring(Prefix.Length, name.Length - Suffix.Length - Prefix.Length);
            IEnumerable<string> words = new[] { name };
            if (!string.IsNullOrEmpty(WordSeparator))
            {
                words = name.Split(new[] { WordSeparator }, StringSplitOptions.RemoveEmptyEntries);
            }

            words = ApplyCapitalization(words);

            return Prefix + string.Join(WordSeparator, words) + Suffix;
        }

        private string EnsureSuffix(string name)
        {
            // If the name already ends with any prefix of the Suffix, only append the suffix of
            // the Suffix not contained in the longest such Suffix prefix.

            for (int i = Suffix.Length; i > 0 ; i--)
            {
                if (name.EndsWith(Suffix.Substring(0, i)))
                {
                    return name + Suffix.Substring(i);
                }
            }

            return name + Suffix;
        }

        private string EnsurePrefix(string name)
        {
            // If the name already starts with any suffix of the Prefix, only prepend the prefix of
            // the Prefix not contained in the longest such Prefix suffix.

            for (int i = 0; i < Prefix.Length; i++)
            {
                if (name.StartsWith(Prefix.Substring(i)))
                {
                    return Prefix.Substring(0, i) + name;
                }
            }

            return Prefix + name;
        }

        private void EnsureNonNullProperties()
        {
            Prefix = Prefix ?? string.Empty;
            Suffix = Suffix ?? string.Empty;
            WordSeparator = WordSeparator ?? string.Empty;
        }
    }
}