using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Micajah.AzureFileService
{
    public static class Helper
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
        /// Registers the specified style sheet for the specified control.
        /// </summary>
        /// <param name="ctl">The control to register style sheet for.</param>
        /// <param name="styleSheetWebResourceName">The name of the server-side resource of the style sheet to register.</param>
        public static void RegisterControlStyleSheet(Page page, string styleSheetWebResourceName)
        {
            if (page != null)
            {
                Type pageType = page.GetType();
                string webResourceUrl = ResourceHandler.GetWebResourceUrl(styleSheetWebResourceName, true);

                if (!page.ClientScript.IsClientScriptBlockRegistered(pageType, webResourceUrl))
                {
                    string script = string.Empty;

                    if (page.Header == null)
                        script = string.Format(CultureInfo.InvariantCulture, "<link type=\"text/css\" rel=\"stylesheet\" href=\"{0}\"></link>", webResourceUrl);
                    else
                    {
                        using (HtmlLink link = new HtmlLink())
                        {
                            link.Href = webResourceUrl;
                            link.Attributes.Add("type", "text/css");
                            link.Attributes.Add("rel", "stylesheet");
                            page.Header.Controls.Add(link);
                        }
                    }

                    page.ClientScript.RegisterClientScriptBlock(pageType, webResourceUrl, script, false);
                }
            }
        }

        public static void LoadProperties(object value, Hashtable table)
        {
            if ((value != null) && (table != null))
            {
                foreach (PropertyInfo p in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (table.ContainsKey(p.Name) && p.CanWrite)
                    {
                        try
                        {
                            p.SetValue(value, table[p.Name], null);
                        }
                        catch (ArgumentException) { }
                        catch (TargetException) { }
                        catch (TargetParameterCountException) { }
                        catch (MethodAccessException) { }
                        catch (TargetInvocationException) { }
                    }
                }
            }
        }

        /// <summary>
        /// Encodes non-US-ASCII characters in a string.
        /// </summary>
        /// <param name="value">A string to encode.</param>
        /// <returns>Encoded string.</returns>
        public static string ToHexString(string value)
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

        #endregion
    }
}
