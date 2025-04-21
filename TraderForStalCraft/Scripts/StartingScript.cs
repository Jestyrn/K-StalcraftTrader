using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using Tesseract;
using WindowsInput;

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
        private const uint MOUSEEVENTF_MOVE = 0x0001;

        private static bool _isStarted;
        private decimal delay;
        private decimal inputSpeed;
        private Dictionary<string, int> data;
        private Random random;

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
                MessageBox.Show("Убедитесь что Вы открыли вкладку с аукционом");
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

                    Point? auctionButtonCoordinateats = FindMatch(screen, TakeTemplate(@"\auctionRecognition.png"));
                    // Нажать на кнопку по координатам (аукцион)

                    for (int i = 0; i < data.Count; i++)
                    {
                        currentItem = currentItemList[i];
                        currentPrice = currentPriceList[i];
                        Point? searchFieldCoordinates = FindMatch(screen, TakeTemplate(@"\searchField.png"));
                        // Добавить дрожание (правее, ниже)
                        MoveMouseSmoothly(searchFieldCoordinates.Value.X, searchFieldCoordinates.Value.Y);
                        // CTRL + A -> Backspace
                        // Ввод текста

                        Point? searchButtonCoordinates = FindMatch(screen, TakeTemplate(@"\searchRecognition.png"));
                        // Добавить дрожание
                        MoveMouseSmoothly(searchButtonCoordinates.Value.X, searchButtonCoordinates.Value.Y);


                        Point? buyOutCoordinates = FindMatch(screen, TakeTemplate(@"\buyoutRecognition.png"));
                        //  Добавить дрожание
                        MoveMouseSmoothly(buyOutCoordinates.Value.X, buyOutCoordinates.Value.Y);

                        Point? leftSideCoordinates = FindMatch(screen, TakeTemplate(@"\leftSide.png"));
                        Point? firstPageCoordinates = FindMatch(screen, TakeTemplate(@"\1pageRecognition.png"));
                        string serachPages;

                        neededArea = MakeScreenshotByCoordinates(firstPageCoordinates.Value.X-50, searchButtonCoordinates.Value.X, firstPageCoordinates.Value.Y-20, firstPageCoordinates.Value.Y+50);
                        using (TesseractEngine tesseract = new TesseractEngine(Directory.GetCurrentDirectory() + @"\Data\traindata", "rus", EngineMode.LstmOnly))
                        {
                            tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
                            tesseract.SetVariable("tessedit_pageseg_mode", "7");

                            var temp = tesseract.Process(ConvertBitmapToPixFast(neededArea));
                            serachPages = temp.GetText();
                            serachPages = serachPages.Split("\n")[0];
                            pages = serachPages.Length + 1;
                        }

                        neededArea = MakeScreenshotByCoordinates(leftSideCoordinates.Value.X+10, searchButtonCoordinates.Value.X, buyOutCoordinates.Value.Y-20, firstPageCoordinates.Value.Y);
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
                        lotPrices.AddRange(GetLotPrices(text));
                        buyPrices.AddRange(GetBuyPrices(text));


                        screen.Dispose();
                        screen = MakeScreenshot();

                        // 6 скролов - 1 группа, 5 групп - 1стр
                        // Получить страницы, и сделать цикл поиска по страницам - pages
                        
                        // условие просмотр - Скролл \ Некст Пейдж
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

                        // Скрол
                        /* 1. Навестись на ползунок
                         * 2. Нажать ЛКМ
                         * 3. Опустить мышь
                         * 4. ОТпустить Лкм
                         * 5. Повтрор
                         */
                    
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
                Thread.Sleep(rand.Next(5, 15));
            }

            mouse_event(MOUSEEVENTF_LEFTDOWN, targetX, targetY, 0, IntPtr.Zero);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, targetX, targetY, 0, IntPtr.Zero);
        }

        private List<int> GetLotPrices(string text)
        {
            string[] temp;
            List<int> result = new List<int>();
            List<string> tempList = new List<string>();
            temp = text.Split('\n');
            tempList.AddRange(temp);

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i] == "" || tempList[i] == " " || tempList[i] == "\n" || tempList[i] == "\r" || tempList[i] == "\r\n" || tempList[i] == "\"\"\r\n" || tempList[i] == "\n\n")
                {
                    tempList.RemoveAt(i);
                    --i;
                }
            }

            if (tempList.Count == 10)
            {
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (!(tempList[j].Contains(" ")))
                    {
                        result.Add(Convert.ToInt32(tempList[j]));
                    }
                    else
                    {
                        result.Add(Convert.ToInt32(tempList[j].Split(" ")[0]));
                    }
                }
            }
            else if (tempList.Count == 20)
            {
                for (int j = 0; j < 10; j++)
                {
                    result.Add(Convert.ToInt32(tempList[j]));
                }
            }
            else
            {
                return null;
            }

                return result;
        }

        private List<int> GetBuyPrices(string text)
        {
            string[] temp;
            List<int> result = new List<int>();
            List<string> tempList = new List<string>();
            temp = text.Split('\n');
            tempList.AddRange(temp);

            for (int i = 0; i < tempList.Count; i++)
            {
                if (tempList[i] == "" || tempList[i] == " " || tempList[i] == "\n" || tempList[i] == "\r" || tempList[i] == "\r\n" || tempList[i] == "\"\"\r\n")
                {
                    tempList.RemoveAt(i);
                    --i;
                }
            }

            if (tempList.Count > 10)
            {
                for (int j = 11; j < 20; j++)
                {
                    result.Add(Convert.ToInt32(tempList[j]));
                }
            }
            else
            {
                for (int j = 0; j < 10; j++)
                {
                    if (tempList[j].Contains(" "))
                    {
                        result.Add(Convert.ToInt32(tempList[j].Split(" ")[1]));
                    }
                    else
                    {
                        result.Add(Convert.ToInt32(tempList[j]));
                    }
                }
            }

            return result;
        }

        private Bitmap MakeScreenshotByCoordinates(int x1, int x2, int y1, int y2)
        {
            Bitmap screen = new Bitmap(x2 - x1, y2 - y1);

            using (Graphics g = Graphics.FromImage(screen))
            {
                g.CopyFromScreen(
                    x1+0,
                    y1+10,
                    0, 0,
                    screen.Size,
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
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    IntPtr row = data.Scan0 + y * data.Stride; // Исправлено: умножение на Stride
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte b = Marshal.ReadByte(row, x * 3);
                        byte g = Marshal.ReadByte(row, x * 3 + 1);
                        byte r = Marshal.ReadByte(row, x * 3 + 2);

                        result[y, x] = new Bgr(b, g, r);
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
