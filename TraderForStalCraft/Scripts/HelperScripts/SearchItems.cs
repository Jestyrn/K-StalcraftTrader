using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.Model;

namespace TraderForStalCraft.Scripts.HelperScripts
{
    public class SearchItems
    {
        private ScreenProcessor _sp;

        public string Name { get; set; }
        public int NeedPrice { get; set; }
        public int Money { get; private set; }

        public int Amount { get; private set; }
        public int Price { get; private set; }

        public int Lots {  get; private set; }
        public int Scroll { get; private set; }
        public int Page { get; private set; }

        private Rectangle LotRectangle;
        private Rectangle PriceRectangle;
        private Rectangle AmountRectangle;

        private int heightLot;

        private bool nextItem = false;

        internal SearchItems(ScreenProcessor screenProcessor)
        {
            // Определить области указанных Rectangle
            LotRectangle = new Rectangle();
            PriceRectangle = new Rectangle();
            AmountRectangle = new Rectangle();
            // Опредлелить длину и высоту лота

            Lots = 9;
            _sp = screenProcessor;
        }

        public void StartSearch(string name, int price, int money) 
        {
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
                // некст страница
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
                // Сделать скролл
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
                    // чтобы купить - надо знать:
                    // 1. кудать нажимать (лот)
                    // 2. кудать нажимать (покупать)
                    //      Посчитать затраты
                    //      Посчитать полученные предметы
                    //      Сохранить данные
                    // 3. кудать нажимать (ок)

                    // Реализовать метод покупки
                    BuyLot();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        // проверка - (лот < желаемо), да - вход в покупку, нет - некст лот
        //
        // сошлось - покупать, нет - некст лот
        // 
        // видимые лоты закончились - скролл (повторить видимые лоты)
        // скроллы закончились - некст пейдж (повторять скроллы)

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

        private void BuyLot()
        {

        }
    }
}
