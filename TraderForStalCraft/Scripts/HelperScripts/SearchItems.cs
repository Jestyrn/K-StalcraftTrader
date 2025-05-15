using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.Model;
using TraderForStalCraft.Proprties;

namespace TraderForStalCraft.Scripts.HelperScripts
{
    public class SearchItems
    {
        private ScreenProcessor _sp;
        private InputEmulator _emulator;
        private ParametesOfLot lot;
        private Bitmap screen;

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

        internal SearchItems(ScreenProcessor screenProcessor)
        {
            // Определить области указанных Rectangle
            LotRectangle = new Rectangle(); /* LX - amount.png, UY - (amount.png.y + amount.png.height) вынести в переменную uy, RX - sortMain.png, DY - uy + heightLot */
            PriceRectangle = new Rectangle(); /* LX - sortMain.png - step(отступ влево), UY - (sortMain.png.y + sortMain.png.height) вынести в переменную uy, RX - sortMain.png - step, DY - uy + heightLot */
            AmountRectangle = new Rectangle(); /* LX - amount.png - step(отступ влево(тут возможно не надо)), UY - (amount.png.y + amount.png.height) вынести в переменную uy, RX - amount.png - step(тут возможно не надо), DY - uy + heightLot */
            // Убедится что точно 40 (отправить на тесты клиенту)
            heightLot = 40;

            Lots = 9;
            // Scrolls = n
            // Pages = m
            _sp = screenProcessor;
            _emulator = new InputEmulator();
            BuyButtons = new List<Rectangle>();
        }

        public void StartSearch(string name, int price, int money) 
        {
            // название предмета введено в поле? Нет - написать
            Name = name;
            NeedPrice = price;
            Money = money;

            LookingAtPage();
            if (nextItem)
                return;
        }

        private void LookingAtPage()
        {
            // Определить количество Page (количество страниц)
            for (int i = 0; i < Page; i++)
            {
                LookingAtScroll();
                if (nextItem)
                    return;
                // скроллы закончились - некст пейдж, затем обнулить скроллы и войти в скроллы
            }
        }

        private void LookingAtScroll()
        {
            // Определить количество скроллов
            for (int i = 0; i < Scroll; i++)
            {
                SearchLots();
                if (nextItem)
                    return;
                // видимые лоты закончились - скролл, затем повторить видимые лоты
                // скроллы закончились - некст пейдж, затем обнулить скроллы и войти в скроллы
            }
        }

        private void SearchLots()
        {
            for(int i = 0;i < Lots; i++)
            {
                LotRectangle = new Rectangle(LotRectangle.X, LotRectangle.Y + (heightLot * i), LotRectangle.Width, LotRectangle.Height);

                // Определить индивидульно PriceRectangle, AmountRectangle

                Amount = _sp.ExtractInt(_sp.CaptureArea(AmountRectangle.X, AmountRectangle.Y, AmountRectangle.X + AmountRectangle.Width, AmountRectangle.Y + AmountRectangle.Height));
                Price = _sp.ExtractInt(_sp.CaptureArea(PriceRectangle.X, PriceRectangle.Y, PriceRectangle.X + PriceRectangle.Width, PriceRectangle.Y + PriceRectangle.Height));

                lot = new ParametesOfLot(
                                        name: Name,
                                        balance: Money,
                                        fullPrice: Price,
                                        unitPrice: Price / Amount,
                                        amount: Amount,
                                        needPrice: NeedPrice
                                        );

                // Релизовать метод EnoughtMoney
                if (EnoughtMoney(Price))
                {

                }
                else
                {
                    nextItem = true;
                }

                if (nextItem)
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
            return false;
        }

        private int CalculatePrice(int amount, int price)
        {
            return 0;
        }

        private bool CheckGuess(int price, int needPrice)
        {
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

            // Сделать скрин после нажатия на лот
            screen = _sp.CaptureScreen();
            // Найти кнопку "Купить"
            BuyButton = _sp.FindMatch(screen, "buy.png");
            // Не нашлась - ошибка
            if ((BuyButton == null) || (BuyButton.IsEmpty) | (BuyButton.X == 0))
            {
                throw new InvalidDataException("Кнопка купить не была найдена, она оказалась пустой");
            }
            // Нажать по найденым координатам (в середину кнопки)
            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(new Rectangle(
                BuyButton.X + (BuyButton.Width / 2),
                BuyButton.Y + (BuyButton.Height / 2),
                BuyButton.Width,
                BuyButton.Height)));
            // Запись нового элемента (где находится кнопка купить для лота n)
            BuyButtons.Add(BuyButton);

            // Сделать скриншот после покупки
            screen = _sp.CaptureScreen();
            // Найти "ОК"
            // - Сделать новый шаблон и проверить его на работаспособность - Ok.png
            OkButton = _sp.FindMatch(screen, "Ok.png");
            // Не нашлось - ошибка
            if ((OkButton == null) || (OkButton.IsEmpty) | (OkButton.X == 0))
            {
                throw new InvalidDataException("Кнопка Ок не была найдена, она оказалась пустой");
            }
            // Нажать по найденным координатам (в середину кнопки)
            await _emulator.MoveMouseAsync(_emulator.RectangleToPoint(new Rectangle(
                OkButton.X + (OkButton.Width / 2),
                OkButton.Y + (OkButton.Height / 2),
                OkButton.Width,
                OkButton.Height)));
            // Сериализация для кнопки "Купить" - переменная matches
        }
    }
}
