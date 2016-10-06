using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Micajah.AzureFileService
{
    public static class ImageExtensions
    {
        #region Constants

        private const int OrientationPropertyItemId = 274; // 0x0112

        #endregion

        #region Private Methods

        private static RotateFlipType GetRotateFlipTypeByOrientation(int orientation)
        {
            switch (orientation)
            {
                case 1:
                default:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rotates, flips, or rotates and flips the System.Drawing.Image by Orientation property value from EXIF data.
        /// </summary>
        /// <param name="image">An image to rotate, flip or rotate and flip.</param>
        /// <returns>true, if the image is rotated or flipped; otherwise false.</returns>
        public static bool RotateFlipByOrientation(this Image image)
        {
            if (image != null)
            {
                if (image.PropertyIdList.Contains(OrientationPropertyItemId))
                {
                    PropertyItem property = image.GetPropertyItem(OrientationPropertyItemId);

                    RotateFlipType rotateFlipType = GetRotateFlipTypeByOrientation(property.Value[0]);

                    if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        image.RotateFlip(rotateFlipType);

                        image.RemovePropertyItem(OrientationPropertyItemId);

                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
