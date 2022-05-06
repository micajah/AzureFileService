namespace Micajah.AzureFileService
{
    public struct FileSearchOptions
    {
        public bool AllFiles { get; set; }

        public string[] ExtensionsFilter { get; set; }

        public bool NegateExtensionsFilter { get; set; }
    }
}
