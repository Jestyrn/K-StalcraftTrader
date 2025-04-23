using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Org.BouncyCastle.Math;
using Tesseract;
using WindowsInput;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TraderForStalCraft.Scripts
{
    internal class StartingScript
    {

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private static bool _isStarted;
        private decimal delay;
        private decimal inputSpeed;
        private Dictionary<string, int> data;
        private Random random;

        string loggerPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public StartingScript(decimal delay, decimal speed)
        {
            inputSpeed = speed;
            this.delay = delay;

            random = new Random();
        }

        public void Start(Dictionary<string, int> data)
        {
            this.data = new Dictionary<string, int>(data);
            _isStarted = true;
            StartingBuying();
        }

        public void Stop()
        {
            _isStarted = false;
        }

        private void StartingBuying()
        {
            bool firstStart = true;
            if(Process.GetProcessesByName("stalcraft").Length > 0)
            {
                string text;

                List<string> currentItemList = new List<string>(data.Keys);
                List<int> currentPriceList = new List<int>(data.Values);

                string currentItem;
                int currentPrice;
                int pages;
                bool checker = false;

                if ((delay == 0 || delay == null) & (inputSpeed == 0 || inputSpeed == null))
                    checker = true;
                

                ScreenDoings screenDoings = new ScreenDoings();
                EmulatorClicks emulator = new EmulatorClicks(delay, inputSpeed, checker);

                while (_isStarted)
                {
                    CvInvoke.Init();
                    Bitmap screen = screenDoings.Screenshot();

                    if (firstStart)
                    {
                        MessageBox.Show("Убедитесь что поле поиска пустое.");
                    }

                    Point? auctionButtonCoordinateats = FindMatch(screen, TakeTemplate(@"\auctionRecognition.png"));
                    if (auctionButtonCoordinateats != null)
                    {
                        emulator.MoveMouseSmoothly(auctionButtonCoordinateats.Value.X, auctionButtonCoordinateats.Value.Y);
                    }

                    for (int i = 0; i < data.Count; i++)
                    {
                        screen = screenDoings.Screenshot();

                        Thread.Sleep(100);

                        currentItem = currentItemList[i];
                        currentPrice = currentPriceList[i];

                        // Сериализация координат (hold - не помогает)
                        Point? searchButtonCoordinates = FindMatch(screen, TakeTemplate(@"\searchRecognition.png")); // координаты кнопки поиска
                        Point? searchFieldCoordinates = FindMatch(screen, TakeTemplate(@"\searchField.png")); // координаты поля поиска

                        if (searchFieldCoordinates == null)
                        {
                            searchFieldCoordinates = new Point(searchButtonCoordinates.Value.X - 70, searchButtonCoordinates.Value.Y);
                        }

                        screen = screenDoings.Screenshot();

                        emulator.MoveMouseSmoothly(searchFieldCoordinates.Value.X, searchFieldCoordinates.Value.Y); // Нажатие на поле поиска
                        emulator.InputSearchText(currentItem); // ввод названия предмета
                        emulator.MoveMouseSmoothly(searchButtonCoordinates.Value.X, searchButtonCoordinates.Value.Y); // нажать на кнопку поиска
                        Point? othersortingCoordinates = FindMatch(screen, TakeTemplate(@"\amountRecognition.png")); // фильтр до сброса (пока не работает)
                        Point? buyOutCoordinates = FindMatch(screen, TakeTemplate(@"\buyoutRecognition.png")); // координаты кнопки поиска по выкупу


                        // отредактировать расстояние по иксу для othersortingCoordinates (другой фильтр), сброс фильтра
                        emulator.MoveMouseSmoothly(othersortingCoordinates.Value.X, othersortingCoordinates.Value.Y);
                        emulator.MoveMouseSmoothly(buyOutCoordinates.Value.X, buyOutCoordinates.Value.Y);
                        emulator.MoveMouseSmoothly(buyOutCoordinates.Value.X, buyOutCoordinates.Value.Y);

                        screen = screenDoings.Screenshot();
                        SearchItems(screen, currentPrice);

                        emulator.MoveMouseSmoothly(searchFieldCoordinates.Value.X, searchFieldCoordinates.Value.Y); // Нажатие на поле поиска
                        emulator.ClearSearchField();
                        emulator.MoveMouseSmoothly(searchButtonCoordinates.Value.X, searchButtonCoordinates.Value.Y); // сброс поиска (нажать на кнопку поиска)

                        firstStart = false;
                        screen.Dispose();
                    }

                    if (_isStarted)
                    {
                        MessageBox.Show("Цикл завершен");
                    }
                }
            }
            else
            {
                MessageBox.Show("Игра не запущена");
                return;
            }
        }

        private void SearchItems(Bitmap screen, int needPrice)
        {
            ScreenDoings sc = new ScreenDoings();
            EmulatorClicks emulate = new EmulatorClicks();

            int RowHeight = 37;
            int PriceAreaWidth = 130;
            int PriceAreaHeight = 37;

            string text;

            var amountTemplate = new Bitmap(TakeTemplate(@"\amountRecognition.png"));
            var priceTemplate = new Bitmap(TakeTemplate(@"\buyoutRecognition.png"));
            var okButton = new Bitmap(TakeTemplate(@"\falseOkButton.png"));
            var confirmButton = new Bitmap(TakeTemplate(@"\buyout.png"));

            var amountPos = FindMatch(screen, amountTemplate, min: true);
            var pricePos = FindMatch(screen, priceTemplate, min: true);
            Point? okButtonPos;
            Point? confirmPos;

            //if (amountPos == null || pricePos == null)
            //return;

            int priceX = pricePos.Value.X + 60;
            int priceY = pricePos.Value.Y + 40;
            int amountX = amountPos.Value.X + 10;

            for (int i = 0; i < 9; i++)
            {
                var priceRect = new Rectangle(
                    priceX,
                    priceY + (RowHeight * i),
                    PriceAreaWidth,
                    PriceAreaHeight);

                using (var priceImage = CropImage(screen, priceRect))
                {


                    using (TesseractEngine tesseract = new TesseractEngine(Directory.GetCurrentDirectory() + @"\Data\traindata", "rus", EngineMode.LstmOnly))
                    {
                        tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
                        tesseract.SetVariable("tessedit_pageseg_mode", "7");

                        var temp = tesseract.Process(sc.ConvertBitmapToPixFast(priceImage));
                        text = temp.GetText();
                    }

                    string filteredPrice = new string(text.Where(char.IsDigit).ToArray());

                    if (string.IsNullOrEmpty(filteredPrice))
                        continue;

                    if (int.TryParse(filteredPrice, out int price))
                    {

                        if (price <= needPrice)
                        {
                            emulate.MoveMouseSmoothly(priceX, priceY + (RowHeight* i) + 5);

                            Thread.Sleep(500);

                            screen = sc.Screenshot();
                            confirmPos = FindMatch(screen, confirmButton);

                            Thread.Sleep(100);
                            emulate.MoveMouseSmoothly(confirmPos.Value.X, confirmPos.Value.Y);

                            screen = sc.Screenshot();
                            Thread.Sleep(100);
                            okButtonPos = FindMatch(screen, okButton);

                            if (okButtonPos != null)
                            {
                                emulate.MoveMouseSmoothly(okButtonPos.Value.X, okButtonPos.Value.Y);
                                Thread.Sleep(500);
                                screen = sc.Screenshot();
                            }
                        }
                    }
                }
            }
        }
        private Bitmap CropImage(Bitmap source, Rectangle rect)
        {
            return source.Clone(rect, source.PixelFormat);
        }

        private Point? FindMatch(Bitmap Screen, Bitmap templateImage, double hold = 0.78, int needMatch = 0, bool min = false, bool max = false, bool center = true)
        {
            using (Image<Bgr, byte> source = ConvertToNeedImage(Screen))
            using (Image<Bgr, byte> template = ConvertToNeedImage(templateImage))
            {
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocation, maxLocation;
                    result.MinMax(out minValues, out maxValues, out minLocation, out maxLocation);

                    if (maxValues[needMatch] > hold)
                    {
                        if (min || max)
                        {
                            center = false;
                        }

                        if (center)
                        {
                            return new Point(
                                maxLocation[0].X + template.Width / 2,
                                maxLocation[0].Y + template.Height / 2);
                        }

                        if (max)
                        {
                            return new Point(
                                maxLocation[0].X,
                                maxLocation[0].Y);
                        }

                        if (min)
                        {
                            return new Point(
                                maxLocation[0].X - template.Width,
                                maxLocation[0].Y - template.Height);
                        }
                    }
                }
            }
            return null;
        }

        private Bitmap TakeTemplate(string templatePath)
        {
            string templatesFolder = Directory.GetCurrentDirectory() + @"\Data\Blueprints";
            Bitmap template = new Bitmap(Image.FromFile(templatesFolder + templatePath));
            return template;
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
    }
}
