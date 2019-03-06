
using System.Linq;
using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// Evaluates whether the string in question is essentially void of meaningful value. Syntactic sugar for
        /// String.IsNullOrWhiteSpace, which is basically what the original String.IsNullOrEmpty
        /// should have been.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string toCheckForNullEmptyOrWhitespace) =>
            string.IsNullOrWhiteSpace(toCheckForNullEmptyOrWhitespace);

        public static string CapitalizeFirstLetter(this string valueToCapitalize)
        {
            if (valueToCapitalize.IsNullOrWhiteSpace())
                return valueToCapitalize;

            if (valueToCapitalize.Trim().ToUpperInvariant().First().Equals(valueToCapitalize.Trim()[0])) // don't mess with it if the first character is already proper case
                return valueToCapitalize;

            return Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(valueToCapitalize.Trim());
        }

        public static string SpaceDelimitTitleCaseText(this string wordsCrammedTogetherToBeSeparatedByTitleCase) =>
            wordsCrammedTogetherToBeSeparatedByTitleCase.IsNullOrWhiteSpace()
                ? string.Empty
                : Regex.Replace(wordsCrammedTogetherToBeSeparatedByTitleCase ?? string.Empty, "([a-z])([A-Z])", "$1 $2");
    }
}