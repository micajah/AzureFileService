using System;

namespace Micajah.AzureFileService
{
    /// <summary>
    /// MIME type groups.
    /// </summary>
    [Flags]
    [Serializable]
    public enum MimeTypeGroups
    {
        None = 0,
        Archive = 1,
        Audio = 2,
        Document = 4,
        Image = 8,
        Message = 16,
        MicrosoftOffice = 32,
        Text = 64,
        Video = 128
    }
}
