using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Micajah.AzureFileService
{
    public static class ControlExtensions
    {
        #region Public Methods

        public static void LoadPropertiesFromRequest(this Control control)
        {
            if (control != null)
            {
                Page page = control.Page;
                byte[] bytes = HttpServerUtility.UrlTokenDecode(page.Request.QueryString["d"]);
                string key = Encoding.UTF8.GetString(bytes);
                Hashtable table = page.Session[key] as Hashtable;
                if (table != null)
                {
                    control.LoadProperties(table);
                }
            }
        }

        public static void LoadProperties(this Control control, Hashtable table)
        {
            if ((control != null) && (table != null))
            {
                foreach (PropertyInfo p in control.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (table.ContainsKey(p.Name) && p.CanWrite)
                    {
                        try
                        {
                            p.SetValue(control, table[p.Name], null);
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
        /// Registers the specified style sheet for the control.
        /// </summary>
        /// <param name="styleSheetWebResourceName">The name of the server-side resource of the style sheet to register.</param>
        public static void RegisterStyleSheet(this Control control, string styleSheetWebResourceName)
        {
            if (control != null)
            {
                Page page = control.Page;
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

        #endregion
    }
}
