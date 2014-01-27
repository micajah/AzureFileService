using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Micajah.AzureFileService
{
    public static class ControlExtensions
    {
        #region Constants

        private const string StyleSheetHtml = "<link type=\"text/css\" rel=\"stylesheet\" href=\"{0}\"></link>";

        #endregion

        #region Public Methods

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
                        script = string.Format(CultureInfo.InvariantCulture, StyleSheetHtml, webResourceUrl);
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
