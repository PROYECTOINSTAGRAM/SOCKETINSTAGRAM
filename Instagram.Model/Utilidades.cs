using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Instagram.Model
{
    public class Utilidades
    {
        /// <summary>
        /// Convertir imagen a base64
        /// </summary>
        /// <param name="image">imagen</param>
        /// <returns></returns>
        public static string ConvertImageToBase64(Bitmap image)
        {
            using (MemoryStream m = new MemoryStream())
            {
                image.Save(m, ImageFormat.Jpeg);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        /// <summary>
        /// Convertir base64 to imagen
        /// </summary>
        /// <param name="base64">imagen en base64</param>
        /// <returns></returns>
        public static Image ConvertBase64ToImagen(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return Image.FromStream(new MemoryStream(bytes));
        }

        /// <summary>
        /// Convertir imagen a 24pp
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            }
            return bmp;
        }
    }
}
