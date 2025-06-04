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
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public List<Rectangls> Matches {  get; private set; }

        public Rectangle SearchField { get; private set; }
        public Rectangle SearchButton { get; private set; }

        public static string pathToFile;
        private bool IsSerialized;
        private Bitmap _screen;
        private ScreenProcessor _sp;
        private FileManager _fileManager;
        private InputEmulator _emulator;

        private string pathLogs = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private Rectangle Window;

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

            GetWindowRect(ptr, out RECT rect);
            Rectangle rectangle = new Rectangle(
                rect.Left,
                rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top);


            if (rectangle.Width == 0)
            {
                throw new InvalidDataException("Не удалось определить область игры");
            }
            else
            {
                _sp.gameWindow = rectangle;
                return rectangle;
            }
        }

        public async Task StartSetup()
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
           Thread.Sleep(200);
           SetupSortingAsync();
        }

        public int GetMoney()
        {
            int money = 0;

            List<Rectangls> commonPoints = new List<Rectangls>(Matches.Where(x => (x.Name == "balance.png") || (x.Name == "searchBtn.png")));

            Rectangle matchBalance = commonPoints.Where(x => x.Name == "balance.png").First().Bounds;
            Rectangle matchSearch = commonPoints.Where(x => x.Name == "searchBtn.png").First().Bounds;

            Rectangle balance = new Rectangle(
                matchBalance.X,
                matchBalance.Y,
                (matchSearch.X + (matchSearch.Width / 2)) - (matchBalance.X + matchBalance.Width),
                matchBalance.Height);

            if (Window.X > balance.X || Window.Y > balance.Y)
                throw new InvalidDataException("Баланс вне области игры");

            if (!balance.IsEmpty)
                money = _sp.ExtractInt(_sp.CaptureArea(
                    balance.X, 
                    balance.Y, 
                    balance.X + balance.Width, 
                    balance.Y + balance.Height));

            return money;
        }

        private bool DetermineStatus()
        {
            if (_fileManager.Exists(pathToFile))
                return true;
            else
                return false;
        }

        private bool StateCoordinates()
        {
            Rectangle searchMatches = new Rectangle();

            try
            {
                searchMatches = Matches.Where(x => x.Name == "searchBtn.png").First().Bounds;
            }
            catch (Exception ex)
            { return false; }

            Rectangle searchFree = _sp.FindMatch(_sp.CaptureGame(), "searchBtn.png");
            int count = 0;

            count = Matches.Count(x => x.Bounds.Width == 0);

            if ((searchFree.X == searchMatches.X) & (count == 0))
                return true;

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
            
            _fileManager.SaveToJson<List<Rectangls>>(pathToFile, Matches);
        }

        private void ReceivSearchAsync()
        {
            SearchButton = Matches.Where(x => x.Name == "searchBtn.png").First().Bounds;
            SearchField = new Rectangle(
                SearchButton.X - SearchButton.Width,
                SearchButton.Y + (SearchButton.Height/2),
                SearchButton.Width,
                SearchButton.Height / 2);

            if ((SearchField.Width == 0) || (SearchButton.Width == 0))
                throw new InvalidDataException("Определены не верные координаты точек для:" +
                    $"Кнопка поиск: X {SearchButton.X}, Wigth {SearchButton.Width}\n" +
                    $"Поле поиска: X {SearchField.X}, Width {SearchField.Width}\n");

            bool check = false;
            
            _emulator.MoveMouseAsync(_emulator.RectangleToPoint(SearchField.X, SearchField.Y));
            Thread.Sleep(50);
            
            while (!check)
            {
                if (((Cursor.Position.X < SearchField.X + 2) | (Cursor.Position.X > SearchField.X + 2)) & ((Cursor.Position.Y < SearchField.Y + 2) | (Cursor.Position.Y > SearchField.Y + 2)))
                {
                    _emulator.ClearFieldAsync();
                    check = true;
                }
                else
                {
                    Thread.Sleep(100);
                    _emulator.MoveMouseAsync(_emulator.RectangleToPoint(SearchField.X, SearchField.Y));
                }
            }
        }

        private void SetupSortingAsync()
        {
            bool isSorted = false;
            int cnt = 0;

            List<Rectangls> needRects = new List<Rectangls>(Matches.Where(x => (x.Name == "WorseSort.png") || (x.Name == "NeedSorting.png") || (x.Name == "sortMain.png")));

            Rectangle worse = needRects.Where(x => x.Name == "WorseSort.png").First().Bounds;
            Rectangle need = needRects.Where(x => x.Name == "NeedSorting.png").First().Bounds;
            Rectangle main = needRects.Where(x => x.Name == "sortMain.png").First().Bounds;

            if (main.IsEmpty)
                throw new InvalidDataException("Кнопка сортировки не была найдена (не возможно установить нужную сортировку)");

            while ((!isSorted) || (cnt != 10))
            {
                if (worse.X != Window.X)
                {
                    _emulator.MoveMouseAsync(_emulator.RectangleToPoint(main.X + (main.Width /2), main.Y + (main.Height/2)));

                    Task.Delay(500);

                    need = _sp.FindMatch(_sp.CaptureGame(), "NeedSorting.png");
                    if (!need.IsEmpty)
                    {
                        isSorted = true;
                        return;
                    }
                }
                else if (need.X != Window.X)
                {
                    isSorted = true;
                    cnt = 0;
                    return;
                }
                else
                {
                    _emulator.MoveMouseAsync(_emulator.RectangleToPoint(main.X + (main.Width / 2), main.Y + (main.Height / 2)));
                    worse = _sp.FindMatch(_sp.CaptureGame(), "WorseSort.png");
                    need = _sp.FindMatch(_sp.CaptureGame(), "NeedSorting.png");
                }

                cnt++;

                if (cnt == 10)
                    throw new Exception("Использованы все попытки(5) на выставление сортировки");

            }
        }
    }
}
