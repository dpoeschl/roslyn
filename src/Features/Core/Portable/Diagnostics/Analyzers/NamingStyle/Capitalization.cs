namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
{
    internal enum Capitalization
    {
        /// <summary>
        /// Each word is capitalized
        /// </summary>
        PascalCase,

        /// <summary>
        /// Every word except the first word is capitalized
        /// </summary>
        CamelCase,

        /// <summary>
        /// Only the first word is capitalized
        /// </summary>
        FirstUpper,

        /// <summary>
        /// Every character is capitalized
        /// </summary>
        AllUpper,

        /// <summary>
        /// No characters are capitalized
        /// </summary>
        AllLower
    }
}