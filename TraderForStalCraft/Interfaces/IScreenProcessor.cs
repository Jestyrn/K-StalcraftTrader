using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderForStalCraft.Proprties;
using TraderForStalCraft.Scripts.HelperScripts;

namespace TraderForStalCraft.Interfaces
{
    internal interface IScreenProcessor
    {
        public Bitmap CaptureScreen();
        public Bitmap CaptureArea(int x1, int y1, int x2, int y2);
        public Bitmap CaptureGame();
        public int ExtractInt(Bitmap source);
        public List<Rectangls> GetMatches(Bitmap source);
        public Dictionary<string, Bitmap> GetTemplates();
    }
}
