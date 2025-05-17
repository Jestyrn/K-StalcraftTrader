using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NPOI.OpenXmlFormats.Dml;
using TraderForStalCraft.Proprties;
using TraderForStalCraft.Scripts.HelperScripts;

namespace TraderForStalCraft.Scripts.MainScripts
{
    internal class CompletePreparation
    {
        public List<Rectangls> Matches {  get; private set; }

        public static string pathToFile;
        private bool IsSerialized;
        private Bitmap _screen;
        private ScreenProcessor _sp;
        private FileManager _fileManager;
        private InputEmulator _emulator;

        private string pathLogs = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private Rectangle Window;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow();
        private static extern Rectangle GetWindowRect(IntPtr hwd, out Rectangle rect);

        public CompletePreparation(ScreenProcessor sp, FileManager fm)
        {
            _sp = sp;
            _fileManager = fm;
            pathToFile = Path.Combine(pathToFile, "Recsts.json");
            Matches = new List<Rectangls>();
            _emulator = new InputEmulator();
            Window = new Rectangle();
            Window = FindWindow();
        }

        private Rectangle FindWindow()
        {
            Process game = Process.GetProcessesByName("stalcraft")[0];
            IntPtr ptr = game.MainWindowHandle;
            Rectangle rect = new Rectangle();

            GetWindowRect(ptr, out rect);

            if (rect.IsEmpty || (rect.X == 0))
            {
                throw new InvalidDataException("Не удалось определить область игры");
            }
            else
            {
                _sp.gameWindow = rect;
                return rect;
            }
        }

        public void StartSetup()
        {
            if (DetermineStatus())
            {
                WithSerialization();

                if (!StateCoordinates())
                    WithoutSerialization();
            }
            else
            {
                WithoutSerialization();
            }

            ReceivSearchAsync();
            SetupSortingAsync();
        }

        public int GetMoney()
        {
            int money = 0;
            Rectangle rect = new Rectangle();

            foreach (var item in Matches)
            {
                if (item.Name == "balance.png")
                {
                    rect = item.Bounds;
                }
            }

            // найти норм корды для "Счет" - нужна именно сумма
            // если х счета больше чем х игры - пизда, что-то не так
            if (rect.X != 0)
                money = _sp.ExtractInt(_sp.CaptureArea(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height));

            return money;
        }

        private bool DetermineStatus()
        {
            if (_fileManager.Exists(pathToFile))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool StateCoordinates()
        {
            Rectangle searchMatches = new Rectangle();
            Rectangle searchFree = _sp.FindMatch(_sp.CaptureGame(), "searchBtn.png");
            int count = 0;

            foreach (var rect in Matches)
            {
                if (rect.Name == "searchBtn.png")
                {
                    searchMatches = rect.Bounds;
                }

                if ((rect.Bounds.X == 0) & (rect.Bounds.Y == 0) & (rect.Bounds.Width == 0) & (rect.Bounds.Height == 0))
                {
                    count++;
                }
            }

            if ((searchFree.X == searchMatches.X) & (count == 0))
            {
                return true;
            }

            return false;
        }

        private void WithSerialization()
        {
            Matches.Clear();
            Matches.AddRange(_fileManager.LoadFromJson<List<Rectangls>>(pathToFile));
        }

        private void WithoutSerialization()
        {
            _screen = _sp.CaptureGame();
            Matches.Clear();
            Matches.AddRange(_sp.GetMatches(_screen));
            // _fileManager.SaveToJson<List<Rectangls>>(pathToFile, matches);
            // сделать сериализацию на каждом этапе получения новых координат (ок, кнопки купить)
        }

        private async Task ReceivSearchAsync()
        {
            Rectangle button = new Rectangle();
            Rectangle field = new Rectangle();

            foreach (Rectangls item in Matches)
            {
                if (item.Name == "searchBtn.png")
                {
                    if (item.Bounds.Width != 0)
                    {
                        button = item.Bounds;
                        field = item.Bounds;
                        field.X = item.Bounds.X - item.Bounds.Width;

                        return;
                    }
                    else
                    {
                        throw new InvalidDataException("Не найдена точка для сериализации - Кнопка поиска\n" +
                                                       "Возможные проблемы: \n" +
                                                       "1. Отсутствует изображение шаблона (searchBtn.png)\n" +
                                                       "2. Не найдено совпадение по шаблону (на скриншоте не было игры))\n\n" +
                                                       "Варианты решения:\n" +
                                                       "1. поменяйте положение окна\n" +
                                                      $"2. вручную удалите файл {pathToFile}\n" +
                                                       "3. переустановите программу");
                    }
                }
            }

            if ((button.X == 0) | (field.X == 0))
            {
                throw new InvalidDataException("Не найдена точка для сериализации - Кнопка поиска\n" +
                                               "Возможные проблемы: \n" +
                                               "1. Отсутствует изображение шаблона (searchBtn.png)\n" +
                                               "2. Не найдено совпадение по шаблону (на скриншоте не было игры))\n\n" +
                                               "Варианты решения:\n" +
                                               "1. поменяйте положение окна\n" +
                                              $"2. вручную удалите файл {pathToFile}\n" +
                                               "3. переустановите программу");
            }

            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(field));
            await _emulator.ClearFieldAsync();
        }

        private async Task SetupSortingAsync()
        {
            bool isSorted = false;
            int cnt = 0;

            Rectangle worse = new Rectangle();
            Rectangle need = new Rectangle();
            Rectangle main = new Rectangle();

            string type = "no";

            while ((!isSorted) | (cnt < 5))
            {
                foreach (Rectangls item in Matches)
                {
                    switch (item.Name)
                    {
                        case "WorseSort.png":
                            if (item.Bounds.X != 0)
                            {
                                worse = item.Bounds;
                                type = "worse";
                            }
                            else
                            {
                                return;
                            }
                            break;

                        case "NeedSorting.png":
                            if (item.Bounds.X != 0)
                            {
                                need = item.Bounds;
                                type = "need";
                            }
                            else
                            {
                                return;
                            }
                            break;

                        case "sortMain.png":
                            if (item.Bounds.X != 0)
                            {
                                main = item.Bounds;
                                break;
                            }
                            else
                            {
                                return;
                            }
                    }
                }


                // посмотреть что получится из верхней проверки (если 0, надо полностью выйти)
                switch (type)
                {
                    case "need":
                        isSorted = true;
                        return;

                    case "worse":
                        if (main.X != 0)
                        {
                            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(main));
                            isSorted = true;
                            break;
                        }
                        else
                        {
                            return; 
                        }

                    case "no":
                        if (main.X != 0)
                        {
                            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(main));
                            _screen = _sp.CaptureGame();
                            worse = _sp.FindMatch(_screen, "WorseSort.png");
                            need = _sp.FindMatch(_screen, "NeedSorting.png");
                            break;
                        }
                        else
                        {
                            return;
                        }
                }

                cnt++;

                if (cnt == 5)
                {
                    throw new InvalidDataException("Неудается определить и настроить сортировку,\n" +
                        "поменяйте положение окна, и запустите снова");
                }
                else if ((main == null) | (main.IsEmpty) | (main == Rectangle.Empty) | (main.X == 0))
                {
                    throw new InvalidDataException("Неудается определить положение сортировки,\n" +
                        "поменяйте положение окна, и запустите снова");
                }
            }
        }
    }
}
