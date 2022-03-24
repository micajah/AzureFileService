using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;

namespace Micajah.AzureFileService
{
    public static class Thumbnail
    {
        // Align
        // 2 -- 3 -- 4 
        // |         |
        // |         |
        // 9    1    5
        // |         |
        // |         |
        // 8 -- 7 -- 6
        // 0 - The original image will be returned as thumbnail if its width and height are not greater than specified values.

        #region Private Methods

        private static void DrawImage(Image image, int x, int y, int width, int height, Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, x, y, width, height);
                graphics.Flush();
            }
        }

        private static void GetAlignPosition(int align, int maxWidth, int maxHeight, int localSizeX, int localSizeY, ref int left, ref int top)
        {
            left = 0;
            top = 0;

            switch (align)
            {
                case 1:
                    left = (maxWidth - localSizeX) / 2;
                    top = (maxHeight - localSizeY) / 2;
                    break;
                case 2:
                    left = 0;
                    top = 0;
                    break;
                case 3:
                    left = (maxWidth - localSizeX) / 2;
                    top = 0;
                    break;
                case 4:
                    left = maxWidth - localSizeX;
                    top = 0;
                    break;
                case 5:
                    left = maxWidth - localSizeX;
                    top = (maxHeight - localSizeY) / 2;
                    break;
                case 6:
                    left = maxWidth - localSizeX;
                    top = maxHeight - localSizeY;
                    break;
                case 7:
                    left = (maxWidth - localSizeX) / 2;
                    top = maxHeight - localSizeY;
                    break;
                case 8:
                    left = 0;
                    top = maxHeight - localSizeY;
                    break;
                case 9:
                    left = 0;
                    top = (maxHeight - localSizeY) / 2;
                    break;
            }
        }

        private static void GetProportionalSize(int originalWidth, int originalHeight, ref int width, ref int height)
        {
            if ((originalWidth == width) && (originalHeight == height))
            {
                return;
            }

            double multiplier = (double)originalHeight / (double)originalWidth;

            if (height <= 0)
            {
                height = Convert.ToInt32(((multiplier > 1) ? (width / multiplier) : (width * multiplier)), CultureInfo.InvariantCulture);
                return;
            }
            else if (width <= 0)
            {
                width = Convert.ToInt32(((multiplier > 1) ? (height * multiplier) : (height / multiplier)), CultureInfo.InvariantCulture);
                return;
            }

            double widthPercent = ((originalWidth > 0) ? (100 * width / (double)originalWidth) : 100);
            double heightPercent = ((originalHeight > 0) ? (100 * height / (double)originalHeight) : 100);

            double currentPercent = ((widthPercent > heightPercent) ? heightPercent : widthPercent);
            if (currentPercent == 0)
            {
                if (heightPercent != 0)
                    currentPercent = heightPercent;
                else
                    currentPercent = widthPercent;
            }
            currentPercent = currentPercent / 100;

            width = Convert.ToInt32(originalWidth * currentPercent, CultureInfo.InvariantCulture);
            height = Convert.ToInt32(originalHeight * currentPercent, CultureInfo.InvariantCulture);
        }

        #endregion

        #region Public Methods

        public static void Create(Stream source, string fileName, int width, int height, int align, Stream output)
        {
            if (output == null)
            {
                return;
            }

            MemoryStream stream = null;
            Image sourceImage = null;
            Bitmap scaledImage = null;
            Bitmap outputImage = null;

            try
            {
                string contentType = MimeType.GetMimeType(fileName);

                if (MimeType.IsHeif(contentType))
                {
                    stream = new MemoryStream();

                    using (var image = new MagickImage(source))
                    {
                        image.Format = MagickFormat.Jpeg;

                        image.Write(stream);
                    }

                    source = stream;
                }

                sourceImage = Image.FromStream(source);

                int sourceWidth = sourceImage.Width;
                int sourceHeight = sourceImage.Height;
                int outputWidth = width;
                int outputHeight = height;

                if (align == 0)
                {
                    if ((((width > 0) && (sourceWidth <= width)) || (width <= 0))
                        && (((height > 0) && (sourceHeight <= height)) || (height <= 0)))
                    {
                        outputWidth = sourceWidth;
                        outputHeight = sourceHeight;
                    }
                }

                GetProportionalSize(sourceWidth, sourceHeight, ref outputWidth, ref outputHeight);

                scaledImage = new Bitmap(outputWidth, outputHeight);
                DrawImage(sourceImage, 0, 0, outputWidth, outputHeight, scaledImage);

                if (align > 0)
                {
                    if (width == 0) width = outputWidth;
                    if (height == 0) height = outputHeight;
                    int maxWidth = ((outputWidth > width) ? outputWidth : width);
                    int maxHeight = ((outputHeight > height) ? outputHeight : height);

                    int x = 0;
                    int y = 0;
                    GetAlignPosition(align, maxWidth, maxHeight, outputWidth, outputHeight, ref x, ref y);

                    outputImage = new Bitmap(maxWidth, maxHeight);
                    DrawImage(scaledImage, x, y, outputWidth, outputHeight, outputImage);
                    outputImage.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else
                    scaledImage.Save(output, System.Drawing.Imaging.ImageFormat.Jpeg);

                output.Position = 0;
            }
            finally
            {
                stream?.Dispose();
                sourceImage?.Dispose();
                scaledImage?.Dispose();
                outputImage?.Dispose();
            }
        }

        #endregion
    }
}