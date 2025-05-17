using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.Model;
using SixLabors.Fonts;
using TraderForStalCraft.Proprties;

namespace TraderForStalCraft.Scripts.HelperScripts
{
    public class SearchItems
    {
        private ScreenProcessor _sp;
        private InputEmulator _emulator;
        private ParametesOfLot lot;
        private Bitmap screen;

        public List<Rectangls> matches;

        public string Name { get; set; }
        public int NeedPrice { get; set; }
        public int Money { get; private set; }

        public int Amount { get; private set; }
        public int Price { get; private set; }

        public int Lots {  get; private set; }
        public int Scroll { get; private set; }
        public int Page { get; private set; }

        private Rectangle BuyButton;
        private Rectangle OkButton;

        private Rectangle LotRectangle;
        private Rectangle PriceRectangle;
        private Rectangle AmountRectangle;

        private List<Rectangle> BuyButtons;

        private int heightLot;

        private bool nextItem = false;
        public bool isRunning;

        internal SearchItems(ScreenProcessor screenProcessor)
        {
            // Убедится что точно 40 (отправить на тесты клиенту) (получается тот же скриншот по размерам или нет) 
            heightLot = 40;

            Lots = 9;
            Scroll = 5;

            // Pages = "Найти, используя метод FindPages"
            Page = 1;

            _sp = screenProcessor;
            _emulator = new InputEmulator();
            BuyButtons = new List<Rectangle>();
            matches = new List<Rectangls>();

            isRunning = true;
        }

        public void StartSearch(string name, int price, int money) 
        {
            Name = name;
            NeedPrice = price;
            Money = money;
            int offset = 0;

            List<Rectangls> amountAndSorting = new List<Rectangls>(matches.Where(x => (x.Name == "amount.png") || (x.Name == "sortMain.png")));

            LotRectangle = amountAndSorting.Where(x => x.Name == "amount.png").First().Bounds;
            AmountRectangle = amountAndSorting.Where(x => x.Name == "sortMain.png").First().Bounds;

            offset = LotRectangle.Y + LotRectangle.Height;

            LotRectangle.Y = offset;
            LotRectangle.Height = offset + heightLot;

            AmountRectangle = new Rectangle(
                LotRectangle.X - (LotRectangle.X - LotRectangle.Width),
                offset,
                LotRectangle.X - LotRectangle.Width,
                offset + heightLot);

            // поправить Y при некорректной работе
            offset = AmountRectangle.Y + AmountRectangle.Height;
            AmountRectangle.X -= offset;
            AmountRectangle.Y = offset;
            AmountRectangle.Width = AmountRectangle.Width + (2 * offset);
            AmountRectangle.Height = offset + heightLot;

            LookingAtPage();

            if (nextItem)
                return;
            else if (!isRunning)
                return;
        }

        private void LookingAtPage()
        {
            for (int i = 0; i < Page; i++)
            {
                // Определить количество страниц (передать в этот цикл)
                // Нажимать на некст пейдж

                LookingAtScroll();
                if (nextItem)
                    return;
                else if (!isRunning)
                    return;

                // скроллы закончились - некст пейдж, затем обнулить скроллы и войти в скроллы
                //      1. Обнулить скроллы
                //      2. Некст пейдж
                //      3. Войти в сколлы
            }
        }

        private void LookingAtScroll()
        {
            for (int i = 0; i < Scroll; i++)
            {
                SearchLots();
                if (nextItem)
                    return;
                else if (!isRunning)
                    return;

                // видимые лоты закончились - скролл, затем повторить видимые лоты
                // Сделать переменную для Scroll
                // Сохранять туда положение скролл бара
                // Сделать доступным для сериализации
                // 
                // 1. Зажать точку
                // 2. Переместить вниз (до следующей точки)
                // 3. Отпустить точку
                // Запсать пройденное количество
                // Передать это количество в SearchLots(), каждый просмотр лотов, делать скролл(количество)
                // Реализовать задумку в SearchLots()

                // скроллы закончились - некст пейдж, затем обнулить скроллы и войти в скроллы
                //      1. Обнулить скроллы
                //      2. Некст пейдж
                //      3. Войти в сколлы
            }
        }

        private void SearchLots()
        {
            for(int i = 0;i < Lots; i++)
            {
                LotRectangle.Y = LotRectangle.Y + (heightLot * i);
                AmountRectangle.Y = AmountRectangle.Y + (heightLot * i);

                Amount = _sp.ExtractInt(_sp.CaptureArea(AmountRectangle.X, AmountRectangle.Y, AmountRectangle.X + AmountRectangle.Width, AmountRectangle.Y + AmountRectangle.Height));
                Price = _sp.ExtractInt(_sp.CaptureArea(PriceRectangle.X, PriceRectangle.Y, PriceRectangle.X + PriceRectangle.Width, PriceRectangle.Y + PriceRectangle.Height));

                // куда-то сохранить это
                lot = new ParametesOfLot(
                                        name: Name,
                                        balance: Money,
                                        fullPrice: Price,
                                        unitPrice: Price / Amount,
                                        amount: Amount,
                                        needPrice: NeedPrice
                                        );

                if (!EnoughtMoney(Price))
                    nextItem = true;

                if (nextItem)
                    return;
                else if (!isRunning)
                    return;

                Price = CalculatePrice(Amount, Price);

                if (CheckGuess(Price, NeedPrice))
                {
                    // Реализовать метод покупки
                    BuyLot();

                    // Сериализация для lot - сохранение подсчитанных данных в JSON (Обновить класс FileManager)

                    lot.SetBoughtTrue();
                    BuyButtons.Clear();
                    return;
                }
                else
                {
                    // Мы ищем новый лот? Да - оставить, Нет - поменять
                    return;
                }

                // видимые лоты закончились - скролл, затем повторить видимые лоты
            }
        }

        private bool EnoughtMoney(int price) 
        {
            // Сравнить найденную стоимость с счетом
            
            // Если (Счет < стоимость)
            //      - false
            // Иначе 
            //      - true

            // Дополнительные проверки
            // price == 0
            // счет == 0

            return false;
        }

        private int CalculatePrice(int amount, int price)
        {
            // Посчитать стоимость за 1ед

            // return price / amount
            // price - проверялось ранее, однозначно price != 0

            // Дополнительные проверки
            // amount == 0
            //
            // Убедится в корректности определения (область, цвет фона)
            // Если все таки корреляция и валидация пройдены нормально - принять amount = 0

            return 0;
        }

        private bool CheckGuess(int price, int needPrice)
        {
            // Проверка (Стоимость за 1 ед < Счет)
            // price однозначно не null (price != 0)
            // Money однозначно не null (money != 0)

            // Проверка прошла успешно
            // return true
            // 
            // Проверка прошла плохо
            // return false
            //
            return false;
        }

        private async Task BuyLot()
        {
            // Нажать на стоимость лота, которую нашли
            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(new Rectangle(
                    PriceRectangle.X + (PriceRectangle.Width / 2),
                    PriceRectangle.Y + (PriceRectangle.Height/ 2),
                    PriceRectangle.Width,
                    PriceRectangle.Height
                    )));

            screen = _sp.CaptureScreen();
            BuyButton = _sp.FindMatch(screen, "buy.png");
            if ((BuyButton == null) || (BuyButton.IsEmpty) | (BuyButton.X == 0))
            {
                throw new InvalidDataException("Кнопка купить не была найдена, она оказалась пустой");
            }
            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(new Rectangle(
                BuyButton.X + (BuyButton.Width / 2),
                BuyButton.Y + (BuyButton.Height / 2),
                BuyButton.Width,
                BuyButton.Height)));

            // Запись нового элемента (где находится кнопка купить для лота n)
            BuyButtons.Add(BuyButton);

            // Кнопка ОК
            screen = _sp.CaptureScreen();
            OkButton = _sp.FindMatch(screen, "Ok.png");
            if ((OkButton == null) || (OkButton.IsEmpty) | (OkButton.X == 0))
            {
                throw new InvalidDataException("Кнопка Ок не была найдена, она оказалась пустой");
            }
            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(new Rectangle(
                OkButton.X + (OkButton.Width / 2),
                OkButton.Y + (OkButton.Height / 2),
                OkButton.Width,
                OkButton.Height)));
        }
    }
}
