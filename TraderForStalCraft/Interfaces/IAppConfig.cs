using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderForStalCraft.Proprties;

namespace TraderForStalCraft.Interfaces
{
    internal interface IAppConfig
    {
        public decimal ActionDelay { get; set; }
        public decimal InputSpeed { get; set; }
        public bool SkipPages { get; set; }
        public List<Product> TrackedItems { get; set; }
    }

    public interface IProduct
    {
        public string Name { get; set; }
        public string Price { get; set; }
    }

    public interface IRectangls
    {
        string Name { get; set; }
        Rectangle Bounds { get; set; } 
    }
}
