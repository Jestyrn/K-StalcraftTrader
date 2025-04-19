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
                    string templatesFolder = Directory.GetCurrentDirectory() + @"\Data\Blueprints";
                    Bitmap screen = MakeScreenshot();
                    Bitmap template = new Bitmap(Image.FromFile(templatesFolder + @"\auctionRecognition.png"));
                    FindAuctionButton(screen, template);
                    

                    /*
                
                    1. узнать запущена ли игра
                    2. убедится в открытии аукциона
                    3. ввод нужного товара
                    4. установка сортировки
                    5. условие стоимости: товар < желаемо
                        + покупаем
                        - ищем дальше
                    6. условие товары в области видимости есть?:
                            условие скролл возможен?
                                + Скролл
                                - Некст стр

                    */
                }
            }
            else
            {
                MessageBox.Show("Игра не запущена");
                return;
            }
        }

        private Bitmap MakeScreenshot()
        {
            string path = Directory.GetCurrentDirectory() + @"\Data\MainCheck.png";

            using (Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            {
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
        }

        private Point? FindAuctionButton(Bitmap Screen, Bitmap templateImage, double hold = 0.8)
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
