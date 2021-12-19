using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace EasyScreenshot
{
    public class ScreenShot
    {
        private Bitmap _bitmap;

        private ScreenShot(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public BitmapImage Image => BitmapToImageSource(_bitmap);
        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;

        public static ScreenShot GetScreen()
        {
            var width = (int)SystemParameters.PrimaryScreenWidth;
            var height = (int)SystemParameters.PrimaryScreenHeight;
            var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, new Size(width, height));
            }

            return new ScreenShot(bitmap);
        }

        public void Cut(Rectangle cropRectangle) => _bitmap = _bitmap.Clone(cropRectangle, _bitmap.PixelFormat);

        public void Save(string filePath) => _bitmap.Save(filePath);
        
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}