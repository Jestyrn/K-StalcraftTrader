using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using NPOI.OpenXmlFormats.Dml;
using TraderForStalCraft.Proprties;
using TraderForStalCraft.Scripts.HelperScripts;

namespace TraderForStalCraft.Scripts.MainScripts
{
    internal class CompletePreparation
    {
        public static string pathToFile;
        private bool IsSerialized;
        private List<Rectangls> matches;
        private Bitmap _screen;
        private ScreenProcessor _sp;
        private FileManager _fileManager;
        private InputEmulator _emulator;

        private string pathLogs = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // 3. обработать поле поиска
        // 4. проеврить сортировку

        public CompletePreparation(ScreenProcessor sp, FileManager fm)
        {
            _sp = sp;
            _fileManager = fm;
            pathToFile = Path.Combine(pathToFile, "Recsts.json");
            matches = new List<Rectangls>();
            _emulator = new InputEmulator();
        }

        public void StartSetup()
        {
            // Узнать где находится окно (сходятся ли координаты элементов + "погрешность"(1-5 px))
            if (DetermineStatus())
                WithSerialization();
            else
                WithoutSerialization();

            ReceivSearchAsync();
            SetupSortingAsync();
        }

        public int GetMoney()
        {
            int money = 0;
            Rectangle rect = new Rectangle();

            foreach (var item in matches)
            {
                if (item.Name == "balance.png")
                {
                    rect = item.Bounds;
                }
            }

            // найти норм корды
            money = _sp.ExtractInt(_sp.CaptureArea(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height));

            return money;
        }

        private bool DetermineStatus()
        {
            Rectangle search = new Rectangle();

            if (_fileManager.Exists(pathToFile))
            {
                _screen = _sp.CaptureScreen();
                search = _sp.FindMatch(_screen, "searchBtn.png");

                if ((search != null) & (!search.IsEmpty) & (search.X != 0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void WithSerialization()
        {
            matches.Clear();
            matches.AddRange(_fileManager.LoadFromJson<List<Rectangls>>(pathToFile));
        }

        private void WithoutSerialization()
        {
            _screen = _sp.CaptureScreen();
            matches.Clear();
            matches.AddRange(_sp.GetMatches(_screen));
            // _fileManager.SaveToJson<List<Rectangls>>(pathToFile, matches);
        }

        private async Task ReceivSearchAsync()
        {
            Rectangle button = new Rectangle();
            Rectangle field = new Rectangle();

            button = Rectangle.Empty;
            field = Rectangle.Empty;

            foreach (Rectangls item in matches)
            {
                if (item.Name == "searchBtn.png")
                {
                    if (item.Bounds.Width != 0)
                    {
                        button = item.Bounds;
                        field = item.Bounds;
                        field.X = item.Bounds.X - item.Bounds.Width;
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
                foreach (Rectangls item in matches)
                {
                    switch (item.Name)
                    {
                        case "WorseSort.png":
                            worse = item.Bounds;
                            type = "worse";
                            break;

                        case "NeedSorting.png":
                            need = item.Bounds;
                            type = "need";
                            break;

                        case "sortMain.png":
                            main = item.Bounds;
                            break;
                    }
                }

                switch (type)
                {
                    case "need":
                        isSorted = true;
                        return;

                    case "worse":
                        await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(main));
                        isSorted = true;
                        break;

                    case "no":
                        await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(main));
                        _screen = _sp.CaptureScreen();
                        worse = _sp.FindMatch(_screen, "WorseSort.png");
                        need = _sp.FindMatch(_screen, "NeedSorting.png");
                        break;
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
