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

    /// <summary>
    /// Represents the different sizes of icon.
    /// </summary>
    [Serializable]
    public enum IconSize
    {
        /// <summary>
        /// The size is 128 x 128 pixels.
        /// </summary>
        Bigger = 128,

        /// <summary>
        /// The size is 48 x 48 pixels.
        /// </summary>
        Normal = 48,

        /// <summary>
        /// The size is 32 x 32 pixels.
        /// </summary>
        Small = 32,

        /// <summary>
        /// The size is 16 x 16 pixels.
        /// </summary>
        Smaller = 16
    }

}
