using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Micajah.AzureFileService
{
    public static class StringExtensions
    {
        #region Private Methods

        /// <summary>
        /// Determines if the character needs to be encoded.
        /// </summary>
        /// <param name="value">A Unicode character.</param>
        /// <returns>true if value needs to be converted; otherwise, false.</returns>
        private static bool NeedToEncode(char value)
        {
            if (value <= 127)
            {
                string reservedChars = "$-_.+!*'(),@=&";
                if (char.IsLetterOrDigit(value) || reservedChars.IndexOf(value) >= 0)
                    return false;
            }
            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Encodes non-US-ASCII characters in a string.
        /// </summary>
        /// <param name="value">A string to encode.</param>
        /// <returns>Encoded string.</returns>
        public static string ToHex(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            UTF8Encoding utf8 = new UTF8Encoding();
            StringBuilder sb = new StringBuilder();

            foreach (char chr in value)
            {
                if (NeedToEncode(chr))
                {
                    byte[] encodedBytes = utf8.GetBytes(chr.ToString());
                    for (int index = 0; index < encodedBytes.Length; index++)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "%{0}", Convert.ToString(encodedBytes[index], 16));
                    }
                }
                else
                    sb.Append(chr);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the string is in the list of specified values.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <param name="values">The array of strings.</param>
        /// <returns>true if the string is in the list of specified values; otherwise, false.</returns>
        public static bool In(this string value, params string[] values)
        {
            return In(value, StringComparison.OrdinalIgnoreCase, values);
        }

        /// <summary>
        /// Determines whether the string is in the list of specified values.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <param name="comparisonType"></param>
        /// <param name="values">The array of strings.</param>
        /// <returns>true if the string is in the list of specified values; otherwise, false.</returns>
        public static bool In(this string value, StringComparison comparisonType, params string[] values)
        {
            return values?.Any(x => string.Compare(value, x, comparisonType) == 0) ?? false;
        }

        #endregion
    }
}
