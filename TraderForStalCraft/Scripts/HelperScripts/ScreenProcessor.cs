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

			for (int i = 0; i < files.Length; i++)
			{
                Bitmap tempBitmap = new Bitmap(files[i]);
				string fileName = Path.GetFileName(files[i]);
				files[i] = fileName;
				templates.Add(files[i], new Bitmap(tempBitmap));
			}

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
				tempRect = FindMatch(source, item.Value);
				matches.Add(new Rectangls
				{
					Name = item.Key,
					Bounds = tempRect
				});
			}

			return matches;
		}

		private Rectangle FindMatch(Bitmap source, Bitmap template)
		{
			Mat sourceMat = Emgu.CV.BitmapExtension.ToMat(source);
			Mat templateMat = Emgu.CV.BitmapExtension.ToMat(template);

			Mat sourceGray = new Mat();
			Mat templateGray = new Mat();
			CvInvoke.CvtColor(sourceMat, sourceGray, ColorConversion.Bgr2Gray);
			CvInvoke.CvtColor(templateMat, templateGray, ColorConversion.Bgr2Gray);

			Mat result = new Mat();

			CvInvoke.MatchTemplate(sourceGray, templateGray, result, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
			double minVal = 0, maxVal = 0, thred = 0.4;
			Point minLoc = new Point();
			Point maxLoc = new Point();

			CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

			if (maxVal >= thred)
			{
				return new Rectangle(maxLoc, template.Size);
			}
			else
			{
				return Rectangle.Empty;
			}
		}

		public Dictionary<string, Bitmap> GetTemplates()
		{
			return templates;
		}
	}
}
