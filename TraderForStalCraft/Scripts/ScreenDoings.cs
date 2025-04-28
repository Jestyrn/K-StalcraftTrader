using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using Tesseract;
using System.Runtime.InteropServices;

namespace TraderForStalCraft.Scripts
{
    internal class ScreenDoings
    {
        private string domainFolder;
        private Bitmap currentScreen;
        public List<string> BlueprintsPaths;
        public Dictionary<string, Bitmap> Templates;
        public Dictionary<string, Rectangle> Points;
        private string[] Files;

        /*  0   \1pageRecognition.png   */
        /*  1   \amountRecognition.png  */
        /*  2   \auctionRecognition.png */
        /*  3   \balanceRecognition.png */
        /*  4   \betRecognition.png     */
        /*  5   \buyout.png             */
        /*  6   \buyoutRecognition.png  */
        /*  7   \buyRecognition.png     */
        /*  8   \confirmRecognition.png */
        /*  9   \falseOkButton.png      */
        /*  10  \leftSide.png           */
        /*  11  \page10.png             */
        /*  12  \page11.png             */
        /*  13  \page12.png             */
        /*  14  \page13.png             */
        /*  15  \page2.png              */
        /*  16  \page3.png              */
        /*  17  \page4.png              */
        /*  18  \page5.png              */
        /*  19  \page6.png              */
        /*  20  \page7.png              */
        /*  21  \page8.png              */
        /*  22  \page9.png              */
        /*  23  \scrollRecognition.png  */
        /*  24  \SearchArea.png         */
        /*  25  \searchField.png        */
        /*  26  \searchRecognition.png  */
        /*  27  \skipToLastPage.png     */
        /*  28  \step.png               */

        public ScreenDoings(Bitmap screenshot = null)
        {
            BlueprintsPaths = new List<string>();
            Templates = new Dictionary<string, Bitmap>();
            Points = new Dictionary<string, Rectangle>();

            domainFolder = Directory.GetCurrentDirectory() + @"\Data\Blueprints";

            Files = Directory.GetFiles(domainFolder);

            List<Bitmap> tempList = new List<Bitmap>();
            tempList.AddRange(ReceivingBitmaps());

            if (Files.Length == tempList.Count)
            {
                for (int i = 0; i < tempList.Count; i++)
                {
                    Templates.Add(Files[i], tempList[i]);
                }
            }

            BlueprintsPaths.AddRange(Files);

            this.currentScreen = screenshot;
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

        public Bitmap Screenshot(int x, int y, int width, int height, bool take)
        {
            Bitmap screen = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(screen))
            {
                g.CopyFromScreen(
                    x,
                    y,
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

        private Image<Bgr, byte> ConvertToNeedImage(Bitmap bitmap)
        {
            // Если формат не 24bppRgb, создаем копию с правильным форматом
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                var converted = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(converted))
                {
                    g.DrawImage(bitmap, 0, 0);
                }
                bitmap = converted;
            }

            Image<Bgr, byte> result = new Image<Bgr, byte>(bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                byte[] buffer = new byte[data.Stride * bitmap.Height];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                for (int y = 0; y < bitmap.Height; y++)
                {
                    int rowOffset = y * data.Stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int pixelOffset = rowOffset + x * 3;
                        byte b = buffer[pixelOffset];
                        byte g = buffer[pixelOffset + 1];
                        byte r = buffer[pixelOffset + 2];
                        result[y, x] = new Bgr(r, g, b);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
            return result;
        }

        public Rectangle FindMatch(Bitmap Screen, Bitmap templateImage, double hold = 0.78)
        {
            using (Image<Bgr, byte> source = ConvertToNeedImage(Screen))
            using (Image<Bgr, byte> template = ConvertToNeedImage(templateImage))
            {
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocation, maxLocation;
                    result.MinMax(out minValues, out maxValues, out minLocation, out maxLocation);

                    if (maxValues[0] > hold)
                    {
                        return new Rectangle(maxLocation[0].X, maxLocation[0].Y, template.Width, template.Height);
                    }
                }
            }
            return Rectangle.Empty;
        }

        public Dictionary<string, Rectangle> GetAllPoints(Bitmap screenshot = null)
        {
            List<Rectangle> rects = new List<Rectangle>();

            for (int i = 0; i < Files.Length; i++)
            {
                Points.Add(Files[i], FindMatch(screenshot, Templates[Files[i]]));
            }

            return Points;
        }

        public Rectangle GetSearchButton(Bitmap screenshot = null)
        {
            return FindMatch(screenshot, Templates[Files[26]]);
        }

        public Bitmap CropImage(Bitmap source, Rectangle rect)
        {
            return source.Clone(rect, source.PixelFormat);
        }
    }
}
