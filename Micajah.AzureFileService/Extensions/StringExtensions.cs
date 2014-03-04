using System;
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
                        sb.AppendFormat("%{0}", Convert.ToString(encodedBytes[index], 16));
                    }
                }
                else
                    sb.Append(chr);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts each word in the the specified string to titlecase.
        /// </summary>
        /// <param name="value">The string to convert to titlecase.</param>
        /// <param name="separator">The string that delimit the substrings in this string.</param>
        /// <returns>The string converted to titlecase.</returns>
        public static string ToTitleCase(this string value, string separator)
        {
            // This method should be synchronized with camelize function from dropzone.js file.
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            string[] parts = value.Split(new string[] { separator }, StringSplitOptions.None);

            for (int x = 0; x < parts.Length; x++)
            {
                string p = parts[x];
                char first = char.ToUpperInvariant(p[0]);
                string rest = p.Substring(1);
                parts[x] = first + rest;
            }

            string result = string.Join(string.Empty, parts);

            return result;
        }

        #endregion
    }
}
