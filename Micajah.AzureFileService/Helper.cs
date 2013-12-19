using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Micajah.AzureFileService
{
    public static class Helper
    {
        #region Internal Methods

        /// <summary>
        /// Registers the specified style sheet for the specified control.
        /// </summary>
        /// <param name="ctl">The control to register style sheet for.</param>
        /// <param name="styleSheetWebResourceName">The name of the server-side resource of the style sheet to register.</param>
        public static void RegisterControlStyleSheet(Page page, string styleSheetWebResourceName)
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
                    HtmlLink link = new HtmlLink();
                    link.Href = webResourceUrl;
                    link.Attributes.Add("type", "text/css");
                    link.Attributes.Add("rel", "stylesheet");
                    page.Header.Controls.Add(link);
                }

                page.ClientScript.RegisterClientScriptBlock(pageType, webResourceUrl, script, false);
            }
        }

        public static void LoadProperties(object obj, Hashtable table)
        {
            if ((obj != null) && (table != null))
            {
                foreach (PropertyInfo p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (table.ContainsKey(p.Name) && p.CanWrite)
                    {
                        try
                        {
                            p.SetValue(obj, table[p.Name], null);
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

        #endregion
    }
}
