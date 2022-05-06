using System;

namespace Micajah.AzureFileService.WebControls
{
    /// <summary>
    /// Represents the different sizes of icon.
    /// </summary>
    [Serializable]
    public enum IconSize : int
    {
        /// <summary>
        /// The size is not defined.
        /// </summary>
        NotSet = 0,

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
