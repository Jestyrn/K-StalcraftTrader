using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Tesseract;
using System.Collections.Concurrent;
using TraderForStalCraft.Interfaces;
using System.Runtime.InteropServices;
using TraderForStalCraft.Scripts.HelperScripts;
using TraderForStalCraft.Proprties;
using System.Diagnostics;
using Emgu.CV.CvEnum;
using NPOI.HSSF.Record;
using Emgu.CV.Reg;
using SharpAvi;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TraderForStalCraft.Scripts.HelperScripts
{
	class ScreenProcessor : IScreenProcessor
    {
		[DllImport("user32.dll")]
		private static extern Rectangle GetRectangle(IntPtr ptr, out Rectangle rect);

		private string directory;
		private Dictionary<string, Bitmap> templates;
		private List<Rectangls> matches;
		private string[] files;
		private TesseractEngine tesEngine;

		public ScreenProcessor(string path)
		{
			string tessDataPath = Directory.GetCurrentDirectory() + @"\Data\traindata\";
			directory = path;
			templates = new Dictionary<string, Bitmap>();
			matches = new List<Rectangls>();
			files = Directory.GetFiles(directory);

			tesEngine = new TesseractEngine(tessDataPath, "rus");
		}

		public Bitmap CaptureScreen()
		{
			Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
			using (Graphics g = Graphics.FromImage(screen))
			{
				g.CopyFromScreen(
					Screen.PrimaryScreen.Bounds.X,
					Screen.PrimaryScreen.Bounds.Y,
					0,0,
					screen.Size,
					CopyPixelOperation.SourceCopy);
			}

			return screen;
		}

		public Bitmap CaptureArea(int x1, int y1, int x2, int y2)
		{
            Bitmap screen = new Bitmap(x2 - x1, y2 - y1);
            using (Graphics g = Graphics.FromImage(screen))
            {
                g.CopyFromScreen(
                    x1,
                    y1,
                    0, 0,
                    screen.Size,
                    CopyPixelOperation.SourceCopy);
            }

            return screen;
        }

		public Bitmap CaptureGame()
		{
			string gameName = "stalcraft";
			Process[] process = Process.GetProcessesByName(gameName);

			if (process.Length == 0)
				throw new InvalidOperationException("Игра не запущена");

			Process gameProc = process[0];

			IntPtr ptr = gameProc.MainWindowHandle;
			Rectangle gameRectangele = new Rectangle();
			GetRectangle(ptr, out gameRectangele);

			return new Bitmap(gameRectangele.Width, gameRectangele.Height);
		}

		public int ExtractInt(Bitmap source)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				source.Save(ms, ImageFormat.Png);
				Pix pix = Pix.LoadFromMemory(ms.ToArray());
				using (var proc = tesEngine.Process(pix))
				{
					string text = new string(proc.GetText().Where(char.IsDigit).ToArray());
					return int.TryParse(text, out int result) ? result : 0;
				}
			}
		}

		public List<Rectangls> GetMatches(Bitmap source)
		{
			Rectangle tempRect = new Rectangle();
			foreach (var item in templates)
			{
				matches.Add(new Rectangls
				{
					Name = item.Key,
					Bounds = Rectangle.Empty
				});
			}

			return matches;
		}

		private Rectangle FindMatch(string file)
		{
			return Rectangle.Empty;
		}

		public Dictionary<string, Bitmap> GetTemplates()
		{
			return templates;
		}
	}
}
