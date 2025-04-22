using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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

        public async void Stop()
        {
            _isStarted = false;
        }

        private void StartingBuying()
        {
            if(Process.GetProcessesByName("stalcraft").Length > 0)
            {
                string text;

                MessageBox.Show("Убедитесь что Вы открыли вкладку с аукционом\n" +
                    "Также убедитесь что поле поиска пустое.");

                List<string> currentItemList = new List<string>(data.Keys);
                List<int> currentPriceList = new List<int>(data.Values);

                string currentItem;
                int currentPrice;
                int pages;

                while (_isStarted)
                {
                    CvInvoke.Init();
                    Bitmap neededArea;
                    Bitmap screen = MakeScreenshot();
                    int stepCount = 0;

                    Point? auctionButtonCoordinateats = FindMatch(screen, TakeTemplate(@"\auctionRecognition.png"));
                    if (auctionButtonCoordinateats != null)
                    {
                        MoveMouseSmoothly(auctionButtonCoordinateats.Value.X, auctionButtonCoordinateats.Value.Y);
                    }

                    for (int i = 0; i < data.Count; i++)
                    {
                        screen = MakeScreenshot();
                        Point? xy = FindMatch(screen, TakeTemplate(@"\step.png"), min: true);
                        // подсчет step

                        currentItem = currentItemList[i];
                        currentPrice = currentPriceList[i];

                        // Сериализация координат (hold - не помогает)
                        Point? searchFieldCoordinates = FindMatch(screen, TakeTemplate(@"\searchField.png"), hold: 0.6);

                        Point? searchButtonCoordinates = FindMatch(screen, TakeTemplate(@"\searchRecognition.png"));
                        MoveMouseSmoothly(searchButtonCoordinates.Value.X, searchButtonCoordinates.Value.Y);

                        MoveMouseSmoothly(searchFieldCoordinates.Value.X, searchFieldCoordinates.Value.Y);
                        screen = MakeScreenshot();
                        InputSearchText(currentItem);

                        MoveMouseSmoothly(searchButtonCoordinates.Value.X, searchButtonCoordinates.Value.Y);
                        
                        Point? othersortingCoordinates = FindMatch(screen, TakeTemplate(@"\amountRecognition.png"));
                        Point? buyOutCoordinates = FindMatch(screen, TakeTemplate(@"\buyoutRecognition.png"));

                        // отредактировать расстояние по иксу для othersortingCoordinates (другой фильтр)
                        MoveMouseSmoothly(othersortingCoordinates.Value.X, othersortingCoordinates.Value.Y);
                        MoveMouseSmoothly(buyOutCoordinates.Value.X, buyOutCoordinates.Value.Y);
                        MoveMouseSmoothly(buyOutCoordinates.Value.X, buyOutCoordinates.Value.Y);

                        // Логирование 

                        Point? minPage = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"), min: true);
                        Point? maxPage = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"), max: true);

                        int wight = maxPage.Value.X - minPage.Value.Y;
                        int heist = maxPage.Value.Y - minPage.Value.Y;

                        screen = MakeScreenshotByCoordinates(minPage.Value.X, searchButtonCoordinates.Value.X, minPage.Value.Y + heist, maxPage.Value.Y+10 + heist+10);

                        pages = FindMaxPages(screen);

                        screen = MakeScreenshot();

                        Point? firstPageCoordinates = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"));
                        Point? leftSideCoordinates = FindMatch(screen, TakeTemplate(@"\leftSide.png"));
                        Point? scrollBarCoordinates = FindMatch(screen, TakeTemplate(@"\scrollRecognition.png"));

                        neededArea = MakeScreenshotByCoordinates(leftSideCoordinates.Value.X+10, scrollBarCoordinates.Value.X, buyOutCoordinates.Value.Y-20, firstPageCoordinates.Value.Y-20);

                        // проверить логгирование
                        neededArea.Save(loggerPath + @"\picture.png", System.Drawing.Imaging.ImageFormat.Png);

                        using (TesseractEngine tesseract = new TesseractEngine(Directory.GetCurrentDirectory() + @"\Data\traindata", "rus", EngineMode.LstmOnly))
                        {
                            tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
                            tesseract.SetVariable("tessedit_pageseg_mode", "7");

                            var temp = tesseract.Process(ConvertBitmapToPixFast(neededArea));
                            text = temp.GetText();
                        }

                        // тестирование

                        neededArea = MakeScreenshotByCoordinates(xy.Value.X, xy.Value.X + 100, xy.Value.Y - 10, firstPageCoordinates.Value.Y + 100);

                        neededArea.Save(loggerPath + @"\picture1.png", System.Drawing.Imaging.ImageFormat.Png);

                        using (TesseractEngine tesseract = new TesseractEngine(Directory.GetCurrentDirectory() + @"\Data\traindata", "rus", EngineMode.LstmOnly))
                        {
                            tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
                            tesseract.SetVariable("tessedit_pageseg_mode", "7");

                            var temp = tesseract.Process(ConvertBitmapToPixFast(neededArea));
                            text = temp.GetText();
                        }

                        List<int> lotPrices = new List<int>();
                        List<int> buyPrices = new List<int>();

                        // обработка исключений
                        // Просмотреть все возможные сценарии
                        // 1. Количество предметов
                        // 2. Правильность обработки
                        // 3. Если вариант выкупа "---"

                        lotPrices.AddRange(GetIntPrice(text, false));
                        buyPrices.AddRange(GetIntPrice(text, true));

                        screen.Dispose();
                        screen = MakeScreenshot();
                        
                        for (int j = 0; j < lotPrices.Count; j++)
                        {
                            if (lotPrices[j] < currentPrice)
                            {
                                // Делаем ставку
                            }
                        }

                        if (buyPrices.Count == 10)
                        {
                            for (int j = 0; j < buyPrices.Count; j++)
                            {
                                if (buyPrices[j] < currentPrice)
                                {
                                    // Покупаем
                                }
                            }
                        }

                        // условие просмотр - Скролл \ Некст Пейдж

                        // Вариант 1
                        // 6 скролов - 1 группа, 5 групп - 1стр
                        // Получить страницы, и сделать цикл поиска по страницам - pages

                        // Вариант 2
                        // получить положение скоролл бара
                        // Получить полжение стрелики вниз
                        // листать пока стрелка.у сильно меньше чем скроллбар.у

                        // Вариант 3 
                        // Получить количество страниц
                        // получить посчитать текущую страницу
                        // Если стр 1 или мы на последней - шаманить с "длина скрол бара" - разные размеры скролл баров - разное кол-во скролов
                        // Иначе "Скрол" - который ниже

                        // Скрол
                        /* 1. Навестись на ползунок
                         * 2. Нажать ЛКМ
                         * 3. Опустить мышь
                         * 4. ОТпустить Лкм
                         * 5. Повтрор
                         */

                        // null exception
                        MoveMouseSmoothly(searchFieldCoordinates.Value.X, searchFieldCoordinates.Value.Y);
                        ClearSearchField();

                        screen.Dispose();
                    }
                }
            }
            else
            {
                MessageBox.Show("Игра не запущена");
                return;
            }
        }

        private List<int> GetIntPrice(object text, bool isPrice = true)
        {
            List<int> price = new List<int>();
            return price;
        }

        private int FindMaxPages(Bitmap screen)
        {
            int pageMax = 1;

            Point? firstPage = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"));
            Point? skipButton = FindMatch(screen, TakeTemplate(@"\skipToLastPage.png"));

            Point? page1Min = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"), min: true);
            Point? page1Max = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"), max: true);

            int lenth = firstPage.Value.X - page1Max.Value.X;
            int heist = page1Max.Value.Y - page1Min.Value.Y;

            if (skipButton == null & firstPage != null)
            {
                Point? page2 = FindMatch(screen, TakeTemplate(@"\page2.png"));
                if (page2 != null) 
                {
                    Point? page3 = FindMatch(screen, TakeTemplate(@"\page3.png"));
                    if (page3 != null)
                    {
                        Point? page4 = FindMatch(screen, TakeTemplate(@"\page4.png"));
                        if (page4 != null)
                        {
                            Point? page5 = FindMatch(screen, TakeTemplate(@"\page5.png"));
                            if (page5 != null)
                            {
                                Point? page6 = FindMatch(screen, TakeTemplate(@"\page6.png"));
                                if (page6 != null)
                                {
                                    Point? page7 = FindMatch(screen, TakeTemplate(@"\page7.png"));
                                    if (page7 != null)
                                    {
                                        Point? page8 = FindMatch(screen, TakeTemplate(@"\page8.png"));
                                        if (page8 != null)
                                        {
                                            Point? page9 = FindMatch(screen, TakeTemplate(@"\page9.png"));
                                            if (page9 != null)
                                            {
                                                Point? page10 = FindMatch(screen, TakeTemplate(@"\page10.png"));
                                                if (page10 != null)
                                                {
                                                    Point? page11 = FindMatch(screen, TakeTemplate(@"\page11.png"));
                                                    if (page11 != null)
                                                    {
                                                        Point? page12 = FindMatch(screen, TakeTemplate(@"\page12.png"));
                                                        if (page12 != null)
                                                        {
                                                            Point? page13 = FindMatch(screen, TakeTemplate(@"\page13.png"));
                                                            if (page13 != null)
                                                            {
                                                                return 13;
                                                            }
                                                            else
                                                            {
                                                                return 12;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            return 11;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        return 10;
                                                    }
                                                }
                                                else
                                                {
                                                    return 9;
                                                }
                                            }
                                            else
                                            {
                                                return 8;
                                            }
                                        }
                                        else
                                        {
                                            return 7;
                                        }
                                    }
                                    else
                                    {
                                        return 6;
                                    }
                                }
                                else
                                {
                                    return 5;
                                }
                            }
                            else
                            {
                                return 4;
                            }
                        }
                        else
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    return 1;
                }
            }
            else if (firstPage == null & skipButton == null)
            {
                MessageBox.Show("Не найдены страницы, перезапустите приложние");
                return 0;
            }
            else
            {
                MoveMouseSmoothly(skipButton.Value.X, skipButton.Value.Y);

                Bitmap searchArea = MakeScreenshotByCoordinates(firstPage.Value.X - (lenth / 2) + (lenth * 40), firstPage.Value.X + (lenth / 2) + (lenth*45), firstPage.Value.Y - (heist / 2), firstPage.Value.Y + (heist / 2));

                using (TesseractEngine tesseract = new TesseractEngine(Directory.GetCurrentDirectory() + @"\Data\traindata", "eng", EngineMode.LstmOnly))
                {
                    tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
                    tesseract.SetVariable("tessedit_pageseg_mode", "7");

                    searchArea = GradeUpBitmap(searchArea);

                    var temp = tesseract.Process(ConvertBitmapToPixFast(searchArea));

                    string tempString = temp.GetText();

                    try
                    {
                        tempString = tempString.Trim('\n');
                        tempString = tempString.Trim('\r');
                        tempString = tempString.Trim('\b');
                        tempString = tempString.Trim();
                        tempString = tempString.Trim(' ');

                        pageMax = Convert.ToInt32(temp.GetText());
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Не удалось распознать последню страницу страницу.");
                        return 0;
                    }
                }
            }

            return pageMax;
        }

        // Добавить задержку как ввод с клавиатуры
        private void ClearSearchField()
        {
            InputSimulator input = new InputSimulator();

            input.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_A);
            Thread.Sleep(random.Next(0, 10));
            input.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.BACK);
        }


        // Интегрировать задержку.
        private void InputSearchText(string currentItem)
        {
            InputSimulator input = new InputSimulator();
            int countSymbols = currentItem.Length;
            char[] chars = currentItem.ToCharArray();

            foreach (var symbol in chars)
            {
                input.Keyboard.TextEntry(symbol);
                Thread.Sleep(random.Next(0, 10));
            }
        }


        // Интегрировать задержку
        private void MoveMouseSmoothly(int targetX, int targetY, int steps = 20)
        {
            var rand = new Random();
            Point current = Cursor.Position;

            for (int i = 1; i <= steps; i++)
            {
                        double ratio = (double)i / steps;
                    int newX = current.X + (int)((targetX - current.X) * ratio);
                    int newY = current.Y + (int)((targetY - current.Y) * ratio);

                    newX += rand.Next(-2, 3);
                    newY += rand.Next(-2, 3);

                    Cursor.Position = new Point((int)newX, (int)newY);
                    Thread.Sleep(rand.Next(10, 30));
            }

            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, targetX, targetY, 0, IntPtr.Zero);
            Thread.Sleep(random.Next(200,300));
            mouse_event(MOUSEEVENTF_LEFTUP, targetX, targetY, 0, IntPtr.Zero);
        }

        private Bitmap MakeScreenshotByCoordinates(int x1, int x2, int y1, int y2)
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

        private Pix ConvertBitmapToPixFast(Bitmap bitmap)
        {
            bitmap = GradeUpBitmap(bitmap);
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                var tt = Pix.LoadFromMemory(memoryStream.ToArray());
                tt.Save(loggerPath + @"\pic.png", Tesseract.ImageFormat.Png);
                return Pix.LoadFromMemory(memoryStream.ToArray()); // Конвертируем в byte[]
            }
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
