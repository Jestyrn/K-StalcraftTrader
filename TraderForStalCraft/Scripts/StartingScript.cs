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
using Microsoft.Extensions.FileSystemGlobbing;
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
        private bool pagesCheckbox;

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

        public void Start(Dictionary<string, int> data, CancellationToken cts, bool pagesCheckbox)
        {
            this.data = new Dictionary<string, int>(data);
            _isStarted = true;
            Logger("Скрипт для закупки запущен");
            this.pagesCheckbox = pagesCheckbox;
            StartingBuying();
        }

        public void Stop()
        {
            Logger("Попытка остановить скрипт");
            _isStarted = false;
        }

        private void StartingBuying()
        {
            if (Process.GetProcessesByName("stalcraft").Length > 0)
            {
                Logger("Игра запущена, проверка пройдена");
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
                Logger("Зафиксированны переменные связанные с задержкой");
                try
                {
                    int startCounter = 0;
                    while (_isStarted)
                    {
                        CvInvoke.Init();
                        Bitmap screen = screenDoings.Screenshot();
                        searchButton = screenDoings.GetSearchButton(screen);

                        matchesFromSerialize = false;

                        Logger("Применены настройки для устройств ввода (мышь и клавиатура)");

                        if (firstStart)
                        {
                            Logger($"Изначальная проверка скрипта (проход {startCounter})");
                            FirstStart(screen, screenDoings, data.Count, emulator, currentItemList, currentPriceList, searchButton);
                        }
                        else
                        {
                            Logger($"Последовательная проверка скрипта (проход {startCounter})");
                            NextStart(screen, screenDoings, data.Count, emulator, currentItemList, currentPriceList, searchButton);
                        }

                        startCounter++;
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger("Вызвана остновка скрипта: Безопастно");
                    MessageBox.Show("Script stopped gracefully");
                }
                catch (ThreadAbortException)
                {
                    Logger("Вызвана остановка скрипта: Вынужденно");
                    Thread.ResetAbort();
                    MessageBox.Show("Script was aborted");
                    Logger("Скрипт пытается перезапустится");
                }
                catch (Exception ex)
                {
                    Logger($"Вызвана остановка скрипта: Инное принуждение, текст ошибки следующий{ex.Message}");
                    MessageBox.Show($"Script error: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Игра не запущена");
                Logger("ПРоверка на запуск игры не пройдена");
                return;
            }
        }

        private void ScriptSearch(Bitmap screen, ScreenDoings screenDoings, EmulatorClicks emulator, string currentItem, int currentPrice)
        {
            Logger("Запуск главного скрипта по поиску (SearchScript)");
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

            Logger("Поготовка к покупке завершена");

            // FindAndBuyLots(screen, emulator, screenDoings, currentPrice);
            BuyLots(screen, emulator, screenDoings, currentPrice);
            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width / 2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height / 2));
            emulator.ClearSearchField();
            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width / 2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height / 2));

            /* Подробности по покупке                                   */
            /* 1. Найти стоимость лота                                  */
            /* 2. Найти стак (если есть)                                */
            /* 3. Посчитать (стоимость / стак(если не найден, то = 1))  */
            /* 4. Сравниваем стоимость (найденная < нужной)             */
            /* -да: Покупаем, обносить фильтр, поиск по новой           */
            /* -нет: Даем три попытки на поиск(если исчерпано - скип)   */

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
            Logger("Выполнена сериализация точек поиска");

            screen.Dispose();
        }

        private void BuyLots(Bitmap screen, EmulatorClicks emulator, ScreenDoings sc, int neededPrice)
        {
            Logger("Вызван метод поиска товаров (поиск нужных точек на экране)");
            int RowHeight = 37;
            int PriceAreaWidth = 130;
            int PriceAreaHeight = 37;
            int stakX;
            string priceOCR;
            string amountOCR;

            Rectangle balanceRect = new Rectangle(
                matches[@"\balanceRecognition.png"].X,
                matches[@"\balanceRecognition.png"].Y,
                matches[@"\scrollRecognition.png"].X - matches[@"\balanceRecognition.png"].X - 30,
                matches[@"\balanceRecognition.png"].Height
                );
            Bitmap balanceBitmap = new Bitmap(sc.CropImage(screen, balanceRect));
            int balance = sc.TesseractDetectNumber(balanceBitmap);

            if (balance == 0)
            {
                Logger("Выход из скрипта, ваш баланс определился как \"0 Рублей\"");
                Stop();
                return;
            }

            Logger($"Баланс Определен как {balance} Рублей");


            int amountX = matches[@"\amountRecognition.png"].X - (matches[@"\amountRecognition.png"].Width / 6);
            int amountY = matches[@"\amountRecognition.png"].Y + (matches[@"\amountRecognition.png"].Width / 7);
            int amountWigh = matches[@"\amountRecognition.png"].Width / 2;
            int amountHeight = matches[@"\amountRecognition.png"].Height * 2;

            int priceX = matches[@"\buyoutRecognition.png"].X - 35;
            int priceY = matches[@"\buyoutRecognition.png"].Y + 25;
            int currentPage = 1;
            int count = 0;

            Rectangle scrollDown = matches[@"\scrollRecognition.png"];

            Logger("Поиск точек для покупки завершен");

            if (pagesCheckbox)
            {
                Rectangle pageCoorinates;
                for (int j = 0; j < 12; j++)
                { 
                    // просмотр видемых лотов
                    for (int i = 0; i < 9; i++)
                    {
                        Logger($"Цикл поиска лотов, ткущий лот {i}");

                        screen = sc.Screenshot();

                        Rectangle amountRect = new Rectangle(
                            amountX,
                            amountY + (RowHeight * i),
                            amountWigh,
                            amountHeight
                            );


                        Rectangle priceRect = new Rectangle(
                        priceX,
                        priceY + (RowHeight * i),
                        PriceAreaWidth,
                        PriceAreaHeight);

                        Logger("Определение области поиска координат");

                        // Протестировать работу "количества предметов".
                        using (Bitmap priceImg = new Bitmap(sc.CropImage(screen, priceRect)))
                        using (Bitmap amountImg = new Bitmap(sc.CropImage(screen, amountRect)))
                        {
                            Logger("Найдены изображения для поиска лотов");

                            int price = sc.TesseractDetectNumber(priceImg);
                            int amount = sc.TesseractDetectNumber(amountImg);

                            if (price == 0)
                            {
                                count++;
                                continue;
                            }
                            if (amount == 0)
                                amount = 1;

                            if (price < balance)
                            {
                                Logger($"Итоговые данные для лота, стоимость:{price}, количество:{amount}, стоимость за 1 ед {price / amount}");
                                price = price / amount;
                                Logger("Стоимость лота меньше баланса");
                                if (price <= neededPrice)
                                {
                                    Rectangle Buyout = new Rectangle();

                                    // Нажать на лот
                                    emulator.MoveMouseSmoothly(priceX, priceY + (RowHeight * i) + 5);
                                    screen = sc.Screenshot();

                                    // Нажать на кнопку "Выкупить" (для начала надо найти)
                                    Buyout = sc.FindMatch(screen, sc.Templates[@"\buyRecognition.png"]);
                                    emulator.MoveMouseSmoothly(Buyout.X + Buyout.Width / 2, Buyout.Y + Buyout.Height / 2);

                                    screen = sc.Screenshot();

                                    if (matchesFromSerialize)
                                    {
                                        Logger("Покупка лота с учетом сериализации");
                                        matches[@"\falseOkButton.png"] = sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]);
                                        emulator.MoveMouseSmoothly(matches[@"\falseOkButton.png"].X + (matches[@"\falseOkButton.png"].Width / 2), matches[@"\falseOkButton.png"].Y + (matches[@"\falseOkButton.png"].Height / 2));
                                        Logger("лот куплен / не хватило средств");
                                    }
                                    else
                                    {
                                        Logger("Покупка лота без сериализации");

                                        // Нажать ОК
                                        if (sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]).X != 0)
                                        {
                                            matches[@"\falseOkButton.png"] = sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]);
                                            emulator.MoveMouseSmoothly(matches[@"\falseOkButton.png"].X + (matches[@"\falseOkButton.png"].Width / 2), matches[@"\falseOkButton.png"].Y + (matches[@"\falseOkButton.png"].Height / 2));
                                        }

                                        Logger("Лот куплен / не хватило средств");
                                    }
                                }
                                else
                                {
                                    ++count;
                                    Logger($"Стоимость оказалась больше чем нужно (осталось попыток на поиск выгодног лота {count}/3) ");
                                }
                            }
                            else
                            {
                                Logger($"Стоимость лота превышает баланс, переходим к следюещему лоту (баланс:{balance} лот:{price})");
                                continue;
                            }
                        }
                    }

                    if (count > 2)
                    {
                        return;
                    }

                    if (currentPage == 13)
                    {
                        currentPage = 1;
                        pageCoorinates = new Rectangle();
                        return;
                    }
                    else
                    {
                        currentPage++;

                        Rectangle tempMatchPage = sc.FindMatch(screen, sc.Templates[@$"\page{currentPage}.png"], 0.9);

                        pageCoorinates = new Rectangle(
                            tempMatchPage.X + (tempMatchPage.Width / 2),
                            tempMatchPage.Y + (tempMatchPage.Height / 2),
                            tempMatchPage.Width,
                            tempMatchPage.Height
                            );
                    }

                    if ((pageCoorinates.X != 0) && (pageCoorinates.Y != 0))
                    {
                        emulator.MoveMouseSmoothly(pageCoorinates.X, pageCoorinates.Y);
                    }
                    else
                    {
                        j = 13;
                        return;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    Logger($"Цикл поиска лотов, ткущий лот {i}");

                    screen = sc.Screenshot();

                    Rectangle amountRect = new Rectangle(
                        amountX,
                        amountY + (RowHeight * i),
                        amountWigh,
                        amountHeight
                        );


                    Rectangle priceRect = new Rectangle(
                    priceX,
                    priceY + (RowHeight * i),
                    PriceAreaWidth,
                    PriceAreaHeight);

                    Logger("Определение области поиска координат");

                    // Протестировать работу "количества предметов".
                    using (Bitmap priceImg = new Bitmap(sc.CropImage(screen, priceRect)))
                    using (Bitmap amountImg = new Bitmap(sc.CropImage(screen, amountRect)))
                    {
                        Logger("Найдены изображения для поиска лотов");

                        int price = sc.TesseractDetectNumber(priceImg);
                        int amount = sc.TesseractDetectNumber(amountImg);

                        if (price == 0)
                        {
                            emulator.MoveMouseSmoothly(priceX, priceY + 50);
                            count++;
                            continue;
                        }
                        if (amount == 0)
                            amount = 1;

                        if (price < balance)
                        {
                            Logger($"Итоговые данные для лота, стоимость:{price}, количество:{amount}, стоимость за 1 ед {price / amount}");
                            price = price / amount;
                            Logger("Стоимость лота меньше баланса");
                            if (price <= neededPrice)
                            {
                                Rectangle Buyout = new Rectangle();

                                // Нажать на лот
                                emulator.MoveMouseSmoothly(priceX, priceY + (RowHeight * i) + 5);
                                screen = sc.Screenshot();

                                // Нажать на кнопку "Выкупить" (для начала надо найти)
                                Buyout = sc.FindMatch(screen, sc.Templates[@"\buyRecognition.png"]);
                                emulator.MoveMouseSmoothly(Buyout.X + Buyout.Width / 2, Buyout.Y + Buyout.Height / 2);

                                screen = sc.Screenshot();

                                if (matchesFromSerialize)
                                {
                                    Logger("Покупка лота с учетом сериализации");
                                    matches[@"\falseOkButton.png"] = sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]);
                                    emulator.MoveMouseSmoothly(matches[@"\falseOkButton.png"].X + (matches[@"\falseOkButton.png"].Width / 2), matches[@"\falseOkButton.png"].Y + (matches[@"\falseOkButton.png"].Height / 2));
                                    Logger("лот куплен / не хватило средств");
                                }
                                else
                                {
                                    Logger("Покупка лота без сериализации");

                                    // Нажать ОК
                                    if (sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]).X != 0)
                                    {
                                        matches[@"\falseOkButton.png"] = sc.FindMatch(screen, sc.Templates[@"\falseOkButton.png"]);
                                        emulator.MoveMouseSmoothly(matches[@"\falseOkButton.png"].X + (matches[@"\falseOkButton.png"].Width / 2), matches[@"\falseOkButton.png"].Y + (matches[@"\falseOkButton.png"].Height / 2));
                                    }

                                    Logger("Лот куплен / не хватило средств");
                                }
                            }
                            else
                            {
                                ++count;
                                Logger($"Стоимость оказалась больше чем нужно (осталось попыток на поиск выгодног лота {count}/3) ");
                            }
                        }
                        else
                        {
                            Logger($"Стоимость лота превышает баланс, переходим к следюещему лоту (баланс:{balance} лот:{price})");
                            return;
                        }
                    }
                }
            }
        }

        private void FirstStart(Bitmap screen, ScreenDoings sc, int count, EmulatorClicks emulator, List<string> listItems, List<int> listPrice, Rectangle searchButton)
        {
            Logger("Вход в \"Первый старт\"");
            Rectangle? rectSer = serialize.LoadData()?["\\searchRecognition.png"];
            Rectangle rectMat = sc.GetSearchButton(screen);

            Logger("Поиск наличия сериализации");
            if (rectSer != null)
            {
                if ((rectSer.Value.X == rectMat.X) & (rectSer.Value.Y == rectMat.Y))
                {
                    Logger("Сериализация найдена");
                    matches = serialize.LoadData();
                    matchesFromSerialize = true;
                    Logger("Сериализация загружена");
                }
                else
                {
                    Logger("Начало поиска данных для сериализации");
                    matches = sc.GetAllPoints(screen);
                    matchesFromSerialize = false;
                    Logger("Поиск точек для сериализации завершен");
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
        }

        private void FirstStartReady(EmulatorClicks emulator, ScreenDoings sc)
        {
            Logger("Подготовка ");
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
        }

        private void NextStart(Bitmap screen, ScreenDoings sc, int count, EmulatorClicks emulator, List<string> listItems, List<int> listPrice, Rectangle searchButton)
        {
            matchesFromSerialize = true;

            emulator.MoveMouseSmoothly(matches[@"\searchField.png"].X + (matches[@"\searchField.png"].Width / 2), matches[@"\searchField.png"].Y + (matches[@"\searchField.png"].Height / 2));
            emulator.ClearSearchField();

            for (int i = 0; i < listPrice.Count; i++)
            {
                ScriptSearch(screen, sc, emulator, listItems[i], listPrice[i]);
            }
        }

        private void Logger(string text)
        {
            string logFullPath = Directory.GetCurrentDirectory() + @$"\Logs";
            if (!Directory.Exists(logFullPath))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @$"\Logs");

            text = $"[{DateTime.Now.ToString("HH:mm:ss")}] - {text} \n";

            string path = loggerPath + @$"{DateTime.Now.ToString("dd.MM.yy")}.txt";
            if (!File.Exists(path))
                File.WriteAllText(path, text);
            else
            {
                text = File.ReadAllText(path) + text;
                File.WriteAllText(path, text);
            }
        }
    }
}