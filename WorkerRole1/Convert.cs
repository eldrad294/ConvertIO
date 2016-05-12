
using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WorkerRole1
{
    class Convert
    {
        public static Byte[] ConvertFile(byte[] serializedImage, string EndExtension)
        {
            EndExtension = EndExtension.ToLower();

            // Read image from file
            using (MagickImage image = new MagickImage(serializedImage))
            {
                // Sets the output format to jpeg
                //image.Format = MagickFormat.Jpeg;

                switch (EndExtension)
                {
                    case "bmp":
                        image.Format = MagickFormat.Bmp;
                        break;
                    case "jpeg":
                        image.Format = MagickFormat.Jpeg;
                        break;
                    case "gif":
                        image.Format = MagickFormat.Gif;
                        break;
                    case "png":
                        image.Format = MagickFormat.Png;
                        break;
                    default:
                        return null;
                }
                return image.ToByteArray();
            }

            //EndExtension = EndExtension.ToLower();
            //Image image = byteArrayToImage(serializedImage);
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    switch (EndExtension)
            //    {
            //        case "bmp":
            //            image.Save(ms, ImageFormat.Bmp);
            //            break;
            //        case "jpeg":
            //            image.Save(ms, ImageFormat.Jpeg);
            //            break;
            //        case "gif":
            //            image.Save(ms, ImageFormat.Gif);
            //            break;
            //        case "png":
            //            image.Save(ms, ImageFormat.Png);
            //            break;
            //        default:
            //            ms.Seek(0, 0);
            //            return null;
            //    }
            //    ms.Seek(0, 0);
            //}
        }

        //convert bytearray to image
        private static Image byteArrayToImage(byte[] byteArrayIn)
        {
            var stream = new MemoryStream(byteArrayIn);
            return Image.FromStream(stream, true);
        }

        private static byte[] imgToByteConverter(Image inImg)
        {
            ImageConverter imgCon = new ImageConverter();
            return (byte[])imgCon.ConvertTo(inImg, typeof(byte[]));
        }
    }
}
