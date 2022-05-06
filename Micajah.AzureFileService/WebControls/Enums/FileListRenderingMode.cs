using System;

namespace Micajah.AzureFileService.WebControls
{
    /// <summary>
    /// Defines the different rendering modes for the Micajah.AzureFileService.WebControls.FileList.
    /// </summary>
    [Serializable]
    public enum FileListRenderingMode : int
    {
        /// <summary>
        /// The control is rendered as simple grid view.
        /// </summary>
        Grid = 0,

        /// <summary>
        /// The control is rendered as files list where one file on top of the other file.
        /// </summary>
        List = 1,

        /// <summary>
        /// The control is rendered as thumbnails list.
        /// </summary>
        Thumbnails = 2,
    }

}
