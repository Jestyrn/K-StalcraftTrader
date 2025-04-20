using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace TraderForStalCraft.Scripts
{
    internal class StartingScript
    {
        private static bool _isStarted;
        public void Start()
        {
            _isStarted = true;
            StartingBuying();
        }

        public async void Stop()
        {
            _isStarted = false;
        }

        private void StartingBuying()
        {
            if(Process.GetProcessesByName("stalcraft").Length > 0)
            {
                MessageBox.Show("Убедитесь что Вы открыли вкладку с аукционом");
                while (_isStarted)
                {
                    CvInvoke.Init();
                    Bitmap screen = MakeScreenshot();

                    Point? auctionButtonCoordinateats = FindMatch(screen, TakeTemplate(@"\auctionRecognition.png"));
                    // Нажать на кнопку по координатам (аукцион)

                    Point? searchFieldCoordinates = FindMatch(screen, TakeTemplate(@"\searchField.png"));
                    // Нажать на кнопку по координатам (поле поиска)
                    // Ввод текста

                    Point? searchButtonCoordinates = FindMatch(screen, TakeTemplate(@"\searchRecognition.png"));
                    // Нажать на кнопку по координатам (кнопка поиска)

                    // Сортировка
                    screen.Dispose();
                    screen = MakeScreenshot();

                    // Посмотреть цены

                    // условие просмотр - Купить \ Ставка \ Скролл \ Некст Пейдж

                    
                    screen.Dispose();
                }
            }
            else
            {
                MessageBox.Show("Игра не запущена");
                return;
            }
        }

        private Bitmap TakeTemplate(string templatePath)
        {
            string templatesFolder = Directory.GetCurrentDirectory() + @"\Data\Blueprints";
            Bitmap template = new Bitmap(Image.FromFile(templatesFolder + templatePath));
            return template;
        }

        private Bitmap MakeScreenshot()
        {
            Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            
            using (Graphics g = Graphics.FromImage(screen))
            {
                g.CopyFromScreen(
                    Screen.PrimaryScreen.Bounds.X,
                    Screen.PrimaryScreen.Bounds.Y,
                    0,0,
                    screen.Size,
                    CopyPixelOperation.SourceCopy);
            }
            return screen;
        }

        private Point? FindMatch(Bitmap Screen, Bitmap templateImage, double hold = 0.8)
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
                        return new Point(
                            maxLocation[0].X + template.Width / 2,
                            maxLocation[0].Y + template.Height / 2);
                    }
                }
            }
            return null;
        }

        private Image<Bgr, byte> ConvertToNeedImage(Bitmap bitmap)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0,0 , bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    IntPtr row = data.Scan0 + y + data.Stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte b = Marshal.ReadByte(row, x * 3);
                        byte g = Marshal.ReadByte(row, x * 3 + 1);
                        byte r = Marshal.ReadByte(row, x * 3 + 2);

                        result[y,x] = new Bgr(b, g, r);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
            return result;
        }
    }
}
