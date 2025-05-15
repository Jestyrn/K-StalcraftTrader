using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderForStalCraft.Interfaces;

namespace TraderForStalCraft.Proprties
{
    public class AppConfig
    {
        public decimal ActionDelay { get; set; }  
        public decimal InputSpeed { get; set; }   
        public bool SkipPages { get; set; }       
        public List<Product> TrackedItems { get; set; } = new();
    }

    public class Product : IProduct
    {
        public string Name { get; set; }
        public string Price { get; set; }
    }

    public class Rectangls : IRectangls
    {
        public string Name {  set; get; }
        public Rectangle Bounds { get; set; }
    }

    public class ParametesOfLot
    {
        public string Name { get; private set; }
        public bool IsBought { get; private set; }
        public int Balance {  get; private set; }
        public int FullPrice { get; private set; }
        public int UnitPrice { get; private set; }
        public int Amount { get; private set; }
        public int NeedPrice { get; private set; }
        public string TimeSearch {  get; private set; }

        public ParametesOfLot(string name, int balance, int fullPrice, int unitPrice, int amount,int needPrice, bool isBought = false)
        {
            Name = name;
            IsBought = isBought;
            Balance = balance;
            FullPrice = fullPrice;
            UnitPrice = unitPrice;
            Amount = amount;
            NeedPrice = needPrice;
            TimeSearch = TimeDetect();
        }

        private string TimeDetect() => DateTime.Now.ToString("d.MM.yy - HH:mm:ss");

        internal void SetBoughtTrue() => IsBought = true;
    } 
}
