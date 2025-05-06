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
}
