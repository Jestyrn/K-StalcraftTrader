using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderForStalCraft.Proprties;

namespace TraderForStalCraft.Interfaces
{
    public interface IScreenCapturer
    {
        Bitmap CaptureFullScreen();
        Bitmap CaptureRegion(Rectangle region);
        Bitmap CaptureArea(int x1, int y1, int x2, int y2);
    }

    public interface ITemplateMatcher
    {
        Rectangle FindMatch(Bitmap source, Bitmap template, double threshold = 0.78);
        List<Rectangls> FindAllTemplates(Bitmap source);
    }

    public interface IOcrEngine
    {
        int ExtractNumber(Bitmap image);
    }
}
