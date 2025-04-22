using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace TraderForStalCraft.Scripts
{
    internal class ScreenDoings
    {
        private string domainFolder;
        public List<string> BlueprintsPaths;
        public Dictionary<string, Point?> Points;
        public Dictionary<string, Bitmap> Templates;
        private string[] Files;

        public ScreenDoings()
        {
            BlueprintsPaths = new List<string>();
            Points = new Dictionary<string, Point?>();
            Templates = new Dictionary<string, Bitmap>();

            domainFolder = Directory.GetCurrentDirectory() + @"\Data\Blueprints";

            Files = Directory.GetFiles(domainFolder);
            ReceivingBitmaps();
        }

        public Bitmap Screenshot()
        {
            Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            using (Graphics g = Graphics.FromImage(screen))
            {
                g.CopyFromScreen(
                    Screen.PrimaryScreen.Bounds.X,
                    Screen.PrimaryScreen.Bounds.Y,
                    0, 0,
                    screen.Size,
                    CopyPixelOperation.SourceCopy);
            }
            return screen;
        }

        public Bitmap Screenshot(int x1, int y1, int x2, int y2)
        {
            // Проверка координат
            if (x2 <= x1 || y2 <= y1)
                throw new ArgumentException("Некорректные координаты: x2 должно быть > x1, y2 > y1");

            // Получаем размеры экрана с учётом DPI
            var screenWidth = Screen.PrimaryScreen.Bounds.Width;
            var screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Корректируем координаты, если они выходят за границы
            x1 = Math.Max(0, Math.Min(x1, screenWidth - 1));
            x2 = Math.Max(x1 + 1, Math.Min(x2, screenWidth));
            y1 = Math.Max(0, Math.Min(y1, screenHeight - 1));
            y2 = Math.Max(y1 + 1, Math.Min(y2, screenHeight));

            // Создаём Bitmap с правильным форматом
            Bitmap screen = new Bitmap(x2 - x1, y2 - y1, PixelFormat.Format24bppRgb);

            // Настраиваем Graphics для высокого качества
            using (Graphics g = Graphics.FromImage(screen))
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                g.SmoothingMode = SmoothingMode.None;

                g.CopyFromScreen(
                    x1,
                    y1,
                    0, 0,
                    new Size(x2 - x1, y2 - y1),
                    CopyPixelOperation.SourceCopy);
            }

            return screen;
        }

        public Pix ConvertBitmapToPixFast(Bitmap bitmap)
        {
            bitmap = GradeUpBitmap(bitmap);
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return Pix.LoadFromMemory(memoryStream.ToArray());
            }
        }


        public Bitmap TakeTemplate(string templatePath)
        {
            string templatesFolder = Directory.GetCurrentDirectory() + @"\Data\Blueprints";
            Bitmap template = new Bitmap(Image.FromFile(templatesFolder + templatePath));
            return template;
        }
        private Bitmap GradeUpBitmap(Bitmap bitmap)
        {
            var processed = new Bitmap(bitmap.Width, bitmap.Height);

            // Увеличиваем контраст и делаем черно-белым
            using (var g = Graphics.FromImage(processed))
            using (var attributes = new System.Drawing.Imaging.ImageAttributes())
            {
                float[][] contrastMatrix = {
            new float[] {2, 0, 0, 0, 0},
            new float[] {0, 2, 0, 0, 0},
            new float[] {0, 0, 2, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {-0.5f, -0.5f, -0.5f, 0, 1}
        };

                attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(contrastMatrix));
                g.DrawImage(bitmap,
                           new Rectangle(0, 0, processed.Width, processed.Height),
                           0, 0, bitmap.Width, bitmap.Height,
                           GraphicsUnit.Pixel, attributes);
            }

            return processed;
        }

        private List<Bitmap> ReceivingBitmaps()
        {
            List<Bitmap> list = new List<Bitmap>();

            for (int i = 0; i < Files.Length; i++)
            {
                Files[i] = @"\" + (Files[i].Split(@"\")[Files[i].Split(@"\").Length - 1]);
                list.Add(GradeUpBitmap(new Bitmap(Image.FromFile(domainFolder + Files[i]))));
            }

            return list;
        }
    }
}
