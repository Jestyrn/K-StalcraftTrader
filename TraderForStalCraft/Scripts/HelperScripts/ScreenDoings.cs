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

namespace TraderForStalCraft.Scripts.HelperScripts
{
    public class ScreenProcessor : IScreenCapturer, ITemplateMatcher, IOcrEngine, IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rectangle rectangle);


        private readonly Dictionary<string, Bitmap> _templates;
        private readonly TesseractEngine _tesseract;
        private bool _disposed;

        public ScreenProcessor(string templatesPath)
        {
            _templates = LoadTemplates(templatesPath);

            // _templates = ConvertTemplates(_templates, templatesPath);

            _tesseract = new TesseractEngine(
                Path.Combine(Directory.GetCurrentDirectory(), @"Data\traindata"),
                "rus",
                EngineMode.LstmOnly);
            _tesseract.SetVariable("tessedit_char_whitelist", "0123456789");
        }

        private Dictionary<string, Bitmap> ConvertTemplates(Dictionary<string, Bitmap> templates, string path)
        {
            Mat tempItem;
            string pathToFile;
            foreach (var item in templates)
            {
                tempItem = item.Value.ToMat();
                pathToFile = Path.Combine(path + $"\\{item.Key}");
                File.Delete(pathToFile);
                tempItem.Save(pathToFile);
                templates[item.Key] = new Bitmap(pathToFile);
            }

            return templates;
        }

        private Dictionary<string, Bitmap> LoadTemplates(string path)
        {
            var templates = new Dictionary<string, Bitmap>();
            foreach (var file in Directory.GetFiles(path))
            {
                using var original = new Bitmap(file);
                templates.Add(Path.GetFileName(file), PreprocessImage(original));
            }
            return templates;
        }

        public Bitmap CaptureFullScreen()
        {
            FindWindow();

            var bounds = Screen.PrimaryScreen.Bounds;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return bitmap;
        }

        public Bitmap CaptureRegion(Rectangle region)
        {
            var bitmap = new Bitmap(region.Width, region.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(region.Location, Point.Empty, region.Size);
            }
            return bitmap;
        }

        public Bitmap CaptureArea(int x1, int y1, int x2, int y2)
        {
            if (x2 <= x1 || y2 <= y1)
                throw new ArgumentException("Invalid coordinates");

            return CaptureRegion(new Rectangle(x1, y1, x2 - x1, y2 - y1));
        }

        public Bitmap CaptureGame()
        {
            Rectangle region = FindWindow();
            var bitmap = new Bitmap(region.Width, region.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(region.Location, Point.Empty, region.Size);
            }
            return bitmap;
        }

        private Rectangle FindWindow()
        {
            const string processName = "stalcraft";

            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                throw new InvalidOperationException($"Процесс {processName} не найден!");
            }

            Process proc = processes[0];
            IntPtr ptr = proc.MainWindowHandle;

            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Окно процесса не найдено (возможно, свёрнуто или скрыто)");
            }

            Rectangle gameRect = new Rectangle();
            GetWindowRect(ptr, ref gameRect);

            if (gameRect.Width <= 0 || gameRect.Height <= 0)
            {
                throw new InvalidOperationException("Некорректные размеры окна");
            }

            Debug.WriteLine($"Найдено окно: X={gameRect.X}, Y={gameRect.Y}, Width={gameRect.Width}, Height={gameRect.Height}");

            return gameRect;
        }

        public Rectangle FindMatch(Bitmap source, Bitmap template, double threshold = 0.8)
        {
            string debugPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "template_debug");
            Directory.CreateDirectory(debugPath);

            using (Mat sourceMat = source.ToMat())
            using (Mat templateMat = template.ToMat())
            using (Mat sourceGray = new Mat())
            using (Mat templateGray = new Mat())
            using (Mat result = new Mat())
            {
                // 1. Конвертируем в grayscale (улучшает точность)
                CvInvoke.CvtColor(sourceMat, sourceGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.CvtColor(templateMat, templateGray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                // 2. Нормализуем яркость/контраст
                CvInvoke.EqualizeHist(sourceGray, sourceGray);
                CvInvoke.EqualizeHist(templateGray, templateGray);

                // 3. Используем более точный метод сравнения
                CvInvoke.MatchTemplate(
                    sourceGray,
                    templateGray,
                    result,
                    Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                // Находим максимальное значение совпадения
                double minVal = 0, maxVal = 0;
                Point minLoc = Point.Empty, maxLoc = Point.Empty;
                CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                Debug.WriteLine($"Max match value: {maxVal}");

                // Возвращаем прямоугольник, если совпадение достаточно хорошее
                return maxVal >= threshold
                    ? new Rectangle(maxLoc, template.Size)
                    : Rectangle.Empty;
            }
        }

        public List<Rectangls> FindAllTemplates(Bitmap source)
        {
            var results = new List<Rectangls>();
            foreach (var template in _templates)
            {
                var rect = FindMatch(source, template.Value);
                if (rect != Rectangle.Empty)
                {
                    results.Add(new Rectangls
                    {
                        Name = template.Key,
                        Bounds = rect
                    });
                }
            }
            return results;
        }

        public int ExtractNumber(Bitmap image)
        {
            using var pix = ConvertBitmapToPix(image);
            using var page = _tesseract.Process(pix);
            var text = new string(page.GetText().Where(char.IsDigit).ToArray());
            return int.TryParse(text, out var number) ? number : 0;
        }

        public Bitmap PreprocessImage(Bitmap source)
        {
            var processed = new Bitmap(source.Width, source.Height);
            using (var g = Graphics.FromImage(processed))
            using (var attributes = new ImageAttributes())
            {
                float[][] colorMatrix = {
                    new[] {2f, 0, 0, 0, 0},
                    new[] {0, 2f, 0, 0, 0},
                    new[] {0, 0, 2f, 0, 0},
                    new[] {0, 0, 0, 1f, 0},
                    new[] {-0.5f, -0.5f, -0.5f, 0, 1f}
                };

                attributes.SetColorMatrix(new ColorMatrix(colorMatrix));
                g.DrawImage(source,
                    new Rectangle(0, 0, processed.Width, processed.Height),
                    0, 0, source.Width, source.Height,
                    GraphicsUnit.Pixel, attributes);
            }
            return processed;
        }

        private Pix ConvertBitmapToPix(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return Pix.LoadFromMemory(ms.ToArray());
        }

        public void Dispose()
        {
            if (_disposed) return;

            _tesseract?.Dispose();
            foreach (var template in _templates.Values)
                template?.Dispose();

            _disposed = true;
        }
    }

    public static class BitmapExtensions
    {
        public static Mat ToMat(this Bitmap bitmap)
        {
            var mat = new Mat(bitmap.Height, bitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3);

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                // Копируем данные напрямую, учитывая Stride
                CvInvoke.CvtColor(
                    new Mat(bitmap.Height, bitmap.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 3, bitmapData.Scan0, bitmapData.Stride),
                    mat,
                    Emgu.CV.CvEnum.ColorConversion.Bgr2Rgb // Конвертируем BGR (Bitmap) в RGB (стандарт для OpenCV)
                );
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            return mat;
        }
    }
}
