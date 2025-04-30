using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using log4net.Repository.Hierarchy;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Math;
using Tesseract;
using TraderForStalCraft.Data.Serialize;
using WindowsInput;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TraderForStalCraft.Scripts
{
    internal class StartingScript
    {
        public bool firstStart = true;
        private Dictionary<string, Rectangle> matches;
        private static bool _isStarted;
        private decimal delay;
        private decimal inputSpeed;
        private Dictionary<string, int> data;
        private Random random;
        private string path = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\PointsSer.json";
        Serialize serialize;
        private bool matchesFromSerialize;

        string loggerPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public StartingScript(decimal delay, decimal speed)
        {
            inputSpeed = speed;
            this.delay = delay;

            random = new Random();
        }

        public StartingScript()
        {

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
            if (Process.GetProcessesByName("stalcraft").Length > 0)
            {
                Logger("Запущен");
                List<string> currentItemList = new List<string>(data.Keys);
                List<int> currentPriceList = new List<int>(data.Values);

                Rectangle searchButton;

                string currentItem;
                int currentPrice;
                bool checker = false;
                serialize = new Serialize(path);

                if ((delay == 0 || delay == null) & (inputSpeed == 0 || inputSpeed == null))
                    checker = true;

                ScreenDoings screenDoings = new ScreenDoings();
                EmulatorClicks emulator = new EmulatorClicks(delay, inputSpeed, checker);
                matches = new Dictionary<string, Rectangle>();

                // найти кнопку аукциона
                // нажать
                // скрин

                while (_isStarted)
                {
                    CvInvoke.Init();
                    Bitmap screen = screenDoings.Screenshot();
                    searchButton = screenDoings.GetSearchButton(screen);

                    matchesFromSerialize = false;

                    if (firstStart)
                    {
                        Logger("Первый заход");
                        FirstStart(screen, screenDoings, data.Count, emulator, currentItemList, currentPriceList, searchButton);
                    }
                    else
                    {
                        Logger("N заход");
                        NextStart(screen, screenDoings, data.Count, emulator, currentItemList, currentPriceList, searchButton);
                    }
                }
            }
            else
            {
                MessageBox.Show("Игра не запущена");
                return;
            }
        }

        private void ScriptSearch(Bitmap screen, ScreenDoings screenDoings, EmulatorClicks emulator, string currentItem, int currentPrice)
        {
            Logger("Основной скрипт поиска");
            // Переработать систему поиска

            screen = screenDoings.Screenshot(); // скриншот

            // а если нет сериализации и в поле что-то есть?
            // а если есть сериализация и в поле что-то есть?

            /* Нажать на поле поиска                                    */
            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width / 2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height / 2));

            /* Ввести текст                                             */
            emulator.InputSearchText(currentItem);

            /* Нажать на кнопку поиска                                  */
            emulator.MoveMouseSmoothly(matches[@"\searchRecognition.png"].X + (matches[@"\searchRecognition.png"].Width / 2), matches[@"\searchRecognition.png"].Y + (matches[@"\searchRecognition.png"].Height / 2));

            /* Обновить скриншот (посмотреть на нужные лоты)            */
            screen = screenDoings.Screenshot();

            // FindAndBuyLots(screen, emulator, screenDoings, currentPrice);

            Logger("До покупки");

            BuyLots(screen, emulator, screenDoings, currentPrice);
            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width/2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height/2));
            emulator.ClearSearchField();
            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width/2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height/2));

            Logger("После покупки");
            /* Подробности по покупке                                   */
            /* 1. Найти стоимость лота                                  */
            /* 2. Найти стак (если есть)                                */
            /* 3. Посчитать (стоимость / стак(если не найден, то = 1))  */
            /* 4. Сравниваем стоимость (найденная < нужной)             */
            /* -да: Покупаем, обносить фильтр, поиск по новой           */
            /* -нет: Даем три попытки на поиск(если исчерпано - скип)   */


            // Скрин стака - рабочий
            // int stakX;
            // Bitmap testAnalys = screenDoings.Screenshot(matches[@"\amountRecognition.png"].X - (matches[@"\amountRecognition.png"].Width / 6), matches[@"\amountRecognition.png"].Y + (matches[@"\amountRecognition.png"].Width / 6), matches[@"\amountRecognition.png"].Width / 4, matches[@"\amountRecognition.png"].Height * 2, true);

            /* Анализ - Посмотреть стак, учет стака,                     */
            /* 1. Посмотреть стак                                        */
            /* 2. Посмотреть стоимость                                   */
            /* (Если есть стак, посчитать стоимость лота)                */
            /* (Сравнить стоимость -> покупаем/непокупаем)               */
            /*                                                           */
            /* Если все сошлось - покупать                               */
            /*                                                           */
            /* Обновить фильтр (x2 по "цена выкупа")                     */
            /*                                                           */
            /* Выгрузить сериализацию                                    */
            /*                                                           */
            /* Нажать на поле поиска и стереть текст                     */

            serialize.SaveData(path, matches);

            screen.Dispose();
            Logger("Выход из скрипта");
        }

        private void BuyLots(Bitmap screen, EmulatorClicks emulator, ScreenDoings sc, int neededPrice)
        {
            Logger("Заход в покупку");
            int RowHeight = 37;
            int PriceAreaWidth = 130;
            int PriceAreaHeight = 37;
            string textOCR;

            int priceX = matches[@"\buyoutRecognition.png"].X - 35;
            int priceY = matches[@"\buyoutRecognition.png"].Y + 25;
            // Определить стак
            //int amountX = amountPos.Value.X + 10;

            int count = 0;

            for (int i = 0; i < 9; i++)
            {
                if (count > 2)
                {
                    return;
                }

                var priceRect = new Rectangle(
                priceX,
                priceY + (RowHeight * i),
                PriceAreaWidth,
                PriceAreaHeight);

                using (Bitmap priceImg = new Bitmap(sc.CropImage(screen, priceRect)))
                {
                    priceImg.Save(loggerPath + @$"\ssas{i}.png", ImageFormat.Png);
                    using (TesseractEngine tesseract = new TesseractEngine(Directory.GetCurrentDirectory() + @"\Data\traindata", "rus", EngineMode.LstmOnly))
                    {
                        tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
                        tesseract.SetVariable("tessedit_pageseg_mode", "7");

                        var temp = tesseract.Process(sc.ConvertBitmapToPixFast(priceImg));
                        textOCR = temp.GetText();
                    }

                    string filteredPrice = new string(textOCR.Where(char.IsDigit).ToArray());

                    // если "" - то скролл/переход на некст страницу

                    Logger($"Дошел до покупки {i}");

                    if (string.IsNullOrEmpty(filteredPrice))
                    {
                        continue;
                    }

                    if (int.TryParse(filteredPrice, out int price))
                    {
                        if (price <= neededPrice)
                        {
                            if (matchesFromSerialize)
                            {
                                Rectangle Buy = new Rectangle();
                                Rectangle OK = new Rectangle();
                                Rectangle Confirm = new Rectangle();

                                // нажать на стоимость null
                                emulator.MoveMouseSmoothly(priceX, priceY + (RowHeight * i) + 5);
                                
                                // нажать на выкуп null
                                emulator.MoveMouseSmoothly(priceX+80, priceY + (RowHeight * i) + 40);

                                // ок null
                                screen = sc.Screenshot();
                                Thread.Sleep(500);
                                OK = sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]);
                                if ((OK.X != 0) & (OK.Y != 0))
                                {
                                    matches[@"\falseOkButton.png"] = OK;
                                    emulator.MoveMouseSmoothly(OK.X + (OK.Width / 2), OK.Y + (OK.Height / 2));
                                    count = 3;
                                    continue;
                                }

                                screen = sc.Screenshot();
                                Thread.Sleep(500);
                                Confirm = sc.FindMatch(screen, sc.Templates[@"\confirmRecognition.png"]);
                                if ((Confirm.X != 0) & (Confirm.Y != 0))
                                {
                                    emulator.MoveMouseSmoothly(Confirm.X + (Confirm.Width / 2), Confirm.Y + (Confirm.Height / 2));
                                }
                            }
                            else
                            {
                                // нажать на стоимость
                                emulator.MoveMouseSmoothly(priceX, priceY + (RowHeight * i) + 5);
                                // скрин
                                Thread.Sleep(500);
                                screen = sc.Screenshot();
                                // записать позицию "выкупить"
                                matches[@"\buyout.png"] = sc.FindMatch(screen, sc.Templates[@"\buyout.png"]);
                                // нажать выкупить
                                emulator.MoveMouseSmoothly(matches[@"\buyout.png"].X + (matches[@"\buyout.png"].Width/2), matches[@"\buyout.png"].Y + (matches[@"\buyout.png"].Height/2));
                                // скрин
                                screen = sc.Screenshot();
                                // записать позицию "подтвердить"
                                matches[@"\confirmRecognition.png"] = sc.FindMatch(screen, sc.Templates[@"\confirmRecognition.png"]);
                                // нажать подтвердить
                                emulator.MoveMouseSmoothly(matches[@"\confirmRecognition.png"].X + (matches[@"\confirmRecognition.png"].Width/2), matches[@"\confirmRecognition.png"].Y + (matches[@"\confirmRecognition.png"].Height/2));
                                // скрин
                                screen = sc.Screenshot();
                                // если появилось "ОК"
                                if (sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]).X != 0)
                                {
                                    // нажать ОК
                                    matches[@"\falseOkButton.png"] = sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]);
                                    emulator.MoveMouseSmoothly(matches[@"\falseOkButton.png"].X + (matches[@"\falseOkButton.png"].Width/2), matches[@"\falseOkButton.png"].Y + (matches[@"\falseOkButton.png"].Height/2));
                                    count = 3;
                                    continue;
                                }
                                else if (sc.FindMatch(screen, sc.Templates[@"\confirmRecognition.png"]).X != 0)
                                {
                                    matches[@"\confirmRecognition.png"] = sc.FindMatch(screen, sc.Templates[@"\confirmRecognition.png"]);
                                    emulator.MoveMouseSmoothly(matches[@"\confirmRecognition.png"].X + (matches[@"\confirmRecognition.png"].Width / 2), matches[@"\confirmRecognition.png"].Y + (matches[@"\confirmRecognition.png"].Height / 2));
                                    continue;
                                }
                            }

                            i = 0;

                            price = int.MaxValue;
                            emulator.MoveMouseSmoothly(matches[@"\buyoutRecognition.png"].X + (matches[@"\buyoutRecognition.png"].Width / 2), matches[@"\buyoutRecognition.png"].Y + (matches[@"\buyoutRecognition.png"].Height / 2));
                            emulator.MoveMouseSmoothly(matches[@"\buyoutRecognition.png"].X + (matches[@"\buyoutRecognition.png"].Width / 2), matches[@"\buyoutRecognition.png"].Y + (matches[@"\buyoutRecognition.png"].Height / 2));
                            continue;
                        }
                        else
                        {
                            count++;
                        }
                    }
                }
            }
        }

        private void FirstStart(Bitmap screen, ScreenDoings sc, int count, EmulatorClicks emulator, List<string> listItems, List<int> listPrice, Rectangle searchButton)
        {
            Logger("Самое начало");
            Rectangle? rectSer = serialize.LoadData()?["\\searchRecognition.png"];
            Rectangle rectMat = sc.GetSearchButton(screen);

            if (rectSer != null)
            {
                if ((rectSer.Value.X == rectMat.X) & (rectSer.Value.Y == rectMat.Y))
                {
                    matches = serialize.LoadData();
                    matchesFromSerialize = true;
                }
                else
                {
                    matches = sc.GetAllPoints(screen);
                    matchesFromSerialize = false;
                }
            }
            else if ((matches.Count != 0) | (rectSer == null))
            {
                matches = sc.GetAllPoints(screen);
                matchesFromSerialize = false;
            }

            FirstStartReady(emulator, sc);

            firstStart = false;

            for (int i = 0; i < listPrice.Count; i++)
            {
                ScriptSearch(screen, sc, emulator, listItems[i], listPrice[i]);
            }
            Logger("Конец начала");
        }

        private void FirstStartReady(EmulatorClicks emulator, ScreenDoings sc)
        {
            Logger("Подготовка");
            Rectangle rectangle;
            try
            {
                rectangle = matches["\\auctionRecognition.png"];
            }
            catch (Exception)
            {
                rectangle = sc.FindMatch(sc.Screenshot(), sc.Templates["\\auctionRecognition.png"]);
            }

            if (rectangle.X != 0)
            {
                emulator.MoveMouseSmoothly(rectangle.X, rectangle.Y);
            }

            emulator.MoveMouseSmoothly(matches[@"\searchRecognition.png"].X + (matches[@"\searchRecognition.png"].Width / 2), matches[@"\searchRecognition.png"].Y + (matches[@"\searchRecognition.png"].Height / 2));
            emulator.MoveMouseSmoothly(matches[@"\betRecognition.png"].X + (matches[@"\betRecognition.png"].Width / 2), matches[@"\betRecognition.png"].Y + (matches[@"\betRecognition.png"].Height / 2));
            emulator.MoveMouseSmoothly(matches[@"\buyoutRecognition.png"].X + (matches[@"\buyoutRecognition.png"].Width / 2), matches[@"\buyoutRecognition.png"].Y + (matches[@"\buyoutRecognition.png"].Height / 2));
            emulator.MoveMouseSmoothly(matches[@"\buyoutRecognition.png"].X + (matches[@"\buyoutRecognition.png"].Width / 2), matches[@"\buyoutRecognition.png"].Y + (matches[@"\buyoutRecognition.png"].Height / 2));
            Logger("Конец подготовки");
        }

        private void NextStart(Bitmap screen, ScreenDoings sc, int count, EmulatorClicks emulator, List<string> listItems, List<int> listPrice, Rectangle searchButton)
        {
            Logger("В заходе");
            matchesFromSerialize = true;

            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width / 2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height / 2));
            emulator.ClearSearchField();

            for (int i = 0; i < listPrice.Count; i++)
            {
                ScriptSearch(screen, sc, emulator, listItems[i], listPrice[i]);
            }
            Logger("выход из захода");
        }
    
        private void Logger(string text)
        {
            text = text + "\n";
            string path = loggerPath + @"\logs.txt";
            if (!File.Exists(path))
                File.WriteAllText(path, text + "\n");
            else
            {
                text += File.ReadAllText(path);
                File.WriteAllText(path, text);
            }
        }
    }
}
