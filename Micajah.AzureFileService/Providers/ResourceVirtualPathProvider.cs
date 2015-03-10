using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// Provides a set of methods that enable a Web application to retrieve resources from a virtual file system.
    /// </summary>
    public sealed class ResourceVirtualPathProvider : VirtualPathProvider
    {
        #region Members

        internal const string ManifestResourceNamePrefix = "Micajah.AzureFileService.Resources.Micajah.AzureFileService";
        internal const string VirtualRootPath = "~/Resources.Micajah.AzureFileService/";
        internal const string VirtualRootShortPath = "~/mafs/";

        private static SortedList s_AllChildren;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ResourceVirtualPathProvider class.
        /// </summary>
        internal ResourceVirtualPathProvider()
            : base()
        {
            EnsureAllChildren();
        }

        #endregion

        #region Private Properties

        private static SortedList AllChildren
        {
            get
            {
                EnsureAllChildren();
                return s_AllChildren;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines whether the resources structure is initialized. If it does not, it initializes.
        /// </summary>
        private static void EnsureAllChildren()
        {
            if (s_AllChildren == null)
            {
                s_AllChildren = new SortedList();
                ResourceVirtualDirectory rootDir = new ResourceVirtualDirectory(VirtualRootShortPath, s_AllChildren);
                s_AllChildren.Add(VirtualRootShortPath.ToLower(CultureInfo.CurrentCulture), rootDir);
            }
        }

        private static bool IsAppResourcePath(string virtualPath)
        {
            if (!VirtualPathUtility.IsAppRelative(virtualPath))
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return virtualPath.StartsWith(VirtualRootShortPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a virtual directory.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual directory.</param>
        /// <returns>A virtual directory.</returns>
        private static ResourceVirtualDirectory GetVirtualDirectory(string virtualPath)
        {
            if (!VirtualPathUtility.IsAppRelative(virtualPath))
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);

            return (AllChildren[virtualPath.ToLower(CultureInfo.CurrentCulture)] as ResourceVirtualDirectory);
        }

        /// <summary>
        /// Gets a virtual file.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>A virtual file.</returns>
        private static ResourceVirtualFile GetVirtualFile(string virtualPath)
        {
            if (!VirtualPathUtility.IsAppRelative(virtualPath))
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);

            return (AllChildren[virtualPath.ToLower(CultureInfo.CurrentCulture)] as ResourceVirtualFile);
        }

        private static string ProcessVirtualPath(string virtualPath)
        {
            string url = virtualPath;
            if ((!string.IsNullOrEmpty(url)) && (HttpContext.Current != null))
            {
                if (!(url.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || url.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
                {
                    if (url.StartsWith("~", StringComparison.OrdinalIgnoreCase)) url = url.Remove(0, 1);
                    if (!url.StartsWith("/", StringComparison.OrdinalIgnoreCase)) url = "/" + url;
                    return url;
                }
            }
            return null;
        }

        #endregion

        #region Internal Methods

        internal static string ConvertToLongVirtualPath(string virtualPath)
        {
            string path = virtualPath;
            if (!VirtualPathUtility.IsAppRelative(path))
                path = VirtualPathUtility.ToAppRelative(path);

            if (path.StartsWith(VirtualRootShortPath, StringComparison.OrdinalIgnoreCase))
                path = path.Replace(VirtualRootShortPath, VirtualRootPath + "Pages/");

            return path;
        }

        internal static string ConvertToShortVirtualPath(string virtualPath)
        {
            string path = virtualPath;
            if (!VirtualPathUtility.IsAppRelative(path))
                path = VirtualPathUtility.ToAppRelative(path);

            if (path.StartsWith(VirtualRootPath + "Pages/", StringComparison.OrdinalIgnoreCase))
                path = path.Replace(VirtualRootPath + "Pages/", VirtualRootShortPath);

            return path;
        }

        /// <summary>
        /// Converts a virtual path to an application absolute path.
        /// </summary>
        /// <param name="virtualPath">The virtual path to convert to an application-relative path.</param>
        /// <returns>The absolute path representation of the specified virtual path or original string, if the conversion is not possible.</returns>
        internal static string VirtualPathToAbsolute(string virtualPath)
        {
            string url = ProcessVirtualPath(virtualPath);
            if (url != null)
            {
                string applicationPath = HttpContext.Current.Request.ApplicationPath;
                if (!applicationPath.Equals("/", StringComparison.OrdinalIgnoreCase))
                {
                    if (!url.Contains(applicationPath + "/")) url = applicationPath + url;
                }
                return url;
            }
            return virtualPath;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Gets a value that indicates whether a file exists in the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>true if the file exists in the virtual file system; otherwise, false.</returns>
        public override bool FileExists(string virtualPath)
        {
            return (IsAppResourcePath(virtualPath) || this.Previous.FileExists(virtualPath));
        }

        /// <summary>
        /// Gets a virtual file from the virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>A descendent of the System.Web.Hosting.VirtualFile class that represents a file in the virtual file system.</returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsAppResourcePath(virtualPath))
                return GetVirtualFile(virtualPath);
            else
                return this.Previous.GetFile(virtualPath);
        }

        /// <summary>
        /// Creates a cache dependency based on the specified virtual paths.
        /// </summary>
        /// <param name="virtualPath">The path to the primary virtual resource.</param>
        /// <param name="virtualPathDependencies">An array of paths to other resources required by the primary virtual resource.</param>
        /// <param name="utcStart">The UTC time at which the virtual resources were read.</param>
        /// <returns>A System.Web.Caching.CacheDependency object for the specified virtual resources.</returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsAppResourcePath(virtualPath)) return null;
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        /// <summary>
        /// Gets a virtual directory from the virtual file system.
        /// </summary>
        /// <param name="virtualDir">The path to the virtual directory.</param>
        /// <returns>A descendent of the System.Web.Hosting.VirtualDirectory class that represents a directory in the virtual file system.</returns>
        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            if (IsAppResourcePath(virtualDir))
                return GetVirtualDirectory(virtualDir);
            else
                return this.Previous.GetDirectory(virtualDir);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers a new ResourceVirtualPathProvider instance with the ASP.NET compilation system.
        /// </summary>
        public static void Register()
        {
            MethodInfo mi = typeof(HostingEnvironment).GetMethod("RegisterVirtualPathProviderInternal", (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod));
            if (mi != null)
                mi.Invoke(null, new object[] { new ResourceVirtualPathProvider() });
            else
                HostingEnvironment.RegisterVirtualPathProvider(new ResourceVirtualPathProvider());
        }

        #endregion
    }

    /// <summary>
    /// Represents a file object in a virtual file or resource space.
    /// </summary>
    internal sealed class ResourceVirtualFile : VirtualFile
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ResourceVirtualFile class.
        /// </summary>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        public ResourceVirtualFile(string virtualPath) : base(virtualPath) { }

        #endregion

        #region Internal Methods

        internal static string GetResourceFileName(string resourceName)
        {
            string[] parts = resourceName.Split('.');
            return string.Join(".", new string[] { parts[parts.Length - 2], parts[parts.Length - 1] });
        }

        #endregion

        #region Overrriden Methods

        /// <summary>
        /// Returns a read-only stream to the virtual resource.
        /// </summary>
        /// <returns>A read-only stream to the virtual file.</returns>
        public override Stream Open()
        {
            string path = ResourceVirtualPathProvider.ConvertToLongVirtualPath(VirtualPath);
            int idx = path.IndexOf('/', 2);
            if (idx > -1) path = path.Substring(idx).Replace('/', '.');
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceVirtualPathProvider.ManifestResourceNamePrefix + path);
        }

        #endregion
    }

    /// <summary>
    /// Represents a directory object in a virtual file or resource space.
    /// </summary>
    internal sealed class ResourceVirtualDirectory : VirtualDirectory
    {
        #region Members

        private static DataTable s_ResourceDataTable;

        private ArrayList m_Children;
        private SortedList m_Directories;
        private SortedList m_Files;

        #endregion

        #region Private Properties

        private static DataTable ResourcesTable
        {
            get
            {
                if (s_ResourceDataTable == null)
                {
                    s_ResourceDataTable = new DataTable();
                    s_ResourceDataTable.Locale = CultureInfo.CurrentCulture;
                    s_ResourceDataTable.Columns.Add("Path", typeof(string));

                    foreach (string resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                    {
                        if (resourceName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts1 = resourceName.Replace(ResourceVirtualPathProvider.ManifestResourceNamePrefix, string.Empty).Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                            string[] parts2 = new string[parts1.Length - 2];
                            Array.Copy(parts1, parts2, parts1.Length - 2);

                            DataRow row = s_ResourceDataTable.NewRow();
                            row["Path"] = ResourceVirtualPathProvider.ConvertToShortVirtualPath(ResourceVirtualPathProvider.VirtualRootPath + string.Join("/", parts2) + "/" + ResourceVirtualFile.GetResourceFileName(resourceName));
                            s_ResourceDataTable.Rows.Add(row);
                        }
                    }
                }

                return s_ResourceDataTable;
            }
        }

        #endregion

        #region Overriden Properties

        /// <summary>
        /// Gets a list of the files and subdirectories contained in this virtual directory.
        /// </summary>
        public override IEnumerable Children
        {
            get { return m_Children; }
        }

        /// <summary>
        /// Gets a list of all the subdirectories contained in this directory.
        /// </summary>
        public override IEnumerable Directories
        {
            get { return m_Directories.GetValueList(); }
        }

        /// <summary>
        /// Gets a list of all files contained in this directory.
        /// </summary>
        public override IEnumerable Files
        {
            get { return m_Files.GetValueList(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ResourceVirtualDirectory class.
        /// </summary>
        /// <param name="virtualPath">The virtual path to the resource represented by this instance.</param>
        /// <param name="children">The collection to add all children of the newly created directory to.</param>
        public ResourceVirtualDirectory(string virtualPath, SortedList children)
            : base(virtualPath)
        {
            m_Children = new ArrayList();
            m_Directories = new SortedList();
            m_Files = new SortedList();

            if (!VirtualPathUtility.IsAppRelative(virtualPath))
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            Initialize(virtualPath, children);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the structure of the virtual directory.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual directory.</param>
        /// <param name="children">The collection to add all children of the newly created directory to.</param>
        private void Initialize(string virtualPath, SortedList children)
        {
            string path = null;
            string fileName = null;
            string cutPath = null;
            string lowerPath = null;
            object obj = null;
            SortedList listObj = m_Files;
            bool isFilePath = true;

            foreach (DataRow row in ResourcesTable.Select(string.Concat("Path LIKE '", virtualPath, "%'")))
            {
                path = row["Path"].ToString();
                fileName = VirtualPathUtility.GetFileName(path);
                cutPath = path.Replace(virtualPath, string.Empty);

                isFilePath = (string.Compare(fileName, cutPath, StringComparison.OrdinalIgnoreCase) == 0);

                if (!isFilePath)
                {
                    path = path.Substring(0, (path.IndexOf('/', virtualPath.Length + 1) + 1));
                    listObj = m_Directories;
                }
                else
                    listObj = m_Files;

                lowerPath = path.ToLower(CultureInfo.CurrentCulture);

                if (!listObj.Contains(lowerPath))
                {
                    if (isFilePath)
                        obj = new ResourceVirtualFile(path);
                    else
                        obj = new ResourceVirtualDirectory(path, children);

                    m_Children.Add(obj);
                    listObj.Add(lowerPath, obj);
                    if (!children.Contains(lowerPath)) children.Add(lowerPath, obj);
                }
            }
        }

        #endregion
    }
}