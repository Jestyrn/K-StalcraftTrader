using System.ComponentModel;
using System.Text.Json;
using OfficeOpenXml;
using TraderForStalCraft.Scripts;
using System.IO;
using LicenseContext = OfficeOpenXml.LicenseContext;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;
using System.Diagnostics;
using TraderForStalCraft.Data.Serialize;

namespace TraderForStalCraft
{
    public partial class MainForm : Form
    {
        private string mind = "";
        private StartingScript script;
        private int step;
        private Thread task;

        public MainForm()
        {
            string pathToSerilize = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Serialize.json";
            string pathToRandomize = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Randomize.json";

            InitializeComponent();

            script = new StartingScript(scrolDelay.Value, inputScrol.Value);

            if (File.Exists(pathToSerilize))
                LoadFromJsonDataGrid(pathToSerilize);
            if (File.Exists(pathToRandomize))
                LoadFromJsonRandomize(pathToRandomize);
        }

        private void dragDropInfoLabel_DragEnter(object sender, DragEventArgs e)
        {
            mind = dragDropInfoLabel.Text;
            dragDropInfoLabel.Text = "Готовы принимать Ваш файл.";
            dragDropPanel.Capture = true;
            e.Effect = DragDropEffects.Copy;
        }

        private void dragDropInfoLabel_DragLeave(object sender, EventArgs e)
        {
            dragDropInfoLabel.Text = mind;
        }

        private void dragDropInfoLabel_DragDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (paths.Length > 1)
            {
                MessageBox.Show("Перенесите 1 файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string path = paths[0];

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Неудается определить путь файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show("файл не существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string extension = Path.GetExtension(path).ToLower();
            if (extension != ".txt" && extension != ".xls" && extension != ".xlsx" && extension != ".csv")
            {
                MessageBox.Show("не верный формат", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadFile(path);
        }

        private void loadItemsButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "(*.txt)|*.txt|(*xls)|*.xls|(*xlsx)|*xlsx|(*.scv)|*.scv";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Успешно", "Путь к файлу выбран", MessageBoxButtons.OK, MessageBoxIcon.None);
                    LoadFile(dialog.FileName);
                }
            }
        }

        private void LoadFile(string path)
        {
            trackedItemsDataGridView.Rows.Clear();
            string extencion = Path.GetExtension(path).ToLower();
            string[] temp;
            string[] data;
            List<Product> products = new List<Product>();
            try
            {
                switch (extencion)
                {
                    case ".txt":
                        data = File.ReadAllLines(path);
                        for (global::System.Int32 i = 0; i < data.Length; i++)
                        {
                            temp = data[i].Split(':');
                            products.Add(new Product
                            {
                                Name = temp[0],
                                Price = temp[1],
                            });
                        }
                        break;

                    case ".xls":
                    case ".xlsx":
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            IWorkbook workbook;

                            // Определяем формат файла
                            if (extencion.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                            {
                                workbook = new XSSFWorkbook(fs); // Для .xlsx
                            }
                            else
                            {
                                workbook = new HSSFWorkbook(fs); // Для .xls
                            }

                            var sheet = workbook.GetSheetAt(0); // Первый лист

                            for (int row = 1; row <= sheet.LastRowNum; row++) // Начинаем с 1 (пропускаем заголовок)
                            {
                                var currentRow = sheet.GetRow(row);
                                if (currentRow == null) continue;

                                products.Add(new Product
                                {
                                    Name = GetCellValue(currentRow.GetCell(0)), // Колонка A
                                    Price = GetCellValue(currentRow.GetCell(1))  // Колонка B
                                });
                            }
                        }
                        break;

                    default:
                        MessageBox.Show($"Формат файла {extencion} не поддерживается");
                        break;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Проверьте все строки, где-то ошибка!");
                return;
            }

            for (int i = 0; i < products.Count; i++)
            {
                trackedItemsDataGridView.Rows.Add(products[i].Name, products[i].Price);
            }
        }

        private string GetCellValue(ICell cell)
        {
            if (cell == null) return string.Empty;

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue.Trim(),
                CellType.Numeric => cell.NumericCellValue.ToString(CultureInfo.InvariantCulture),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                _ => cell.ToString().Trim()
            };
        }

        private void SaveDataButton_Click(object sender, EventArgs e)
        {
            List<Product> products = new List<Product>();

            for (int i = 0; i < trackedItemsDataGridView.Rows.Count; i++)
            {
                products.Add(new Product
                {
                    Name = trackedItemsDataGridView[0, i].Value.ToString(),
                    Price = trackedItemsDataGridView[1, i].Value.ToString(),
                });
            }

            SaveToJsonFile(products);
        }

        private void SaveToJsonFile(List<Product> products)
        {
            string baseDirectory = Directory.GetCurrentDirectory();
            string filePath = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Serialize.json";
            string json;
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // Красивое форматирование
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Для кириллицы
                };
                json = JsonSerializer.Serialize(products, options);

                if (!File.Exists(baseDirectory))
                    Directory.CreateDirectory(baseDirectory + "\\Data\\");
                if (!File.Exists(baseDirectory + "\\Data\\"))
                    Directory.CreateDirectory(baseDirectory + "\\Data\\Serialize\\");
                File.WriteAllText(filePath, json);
            }
            catch
            {
                MessageBox.Show("Ошибка", "Не удолось сохранить данные.");
                return;
            }

        }

        private void LoadFromJsonDataGrid(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var returned = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();

            for (int i = 0; i < returned.Count; i++)
            {
                trackedItemsDataGridView.Rows.Add(returned[i].Name, returned[i].Price);
            }
        }

        private void DeleteDataButton_Click(object sender, EventArgs e)
        {
            string filePath = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Serialize.json";
            if (File.Exists(filePath))
                File.Delete(filePath);
            trackedItemsDataGridView.Rows.Clear();
        }

        private void LoadFromJsonRandomize(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var returned = JsonSerializer.Deserialize<DataRandom>(json) ?? new DataRandom();

            scrolDelay.Value = returned.Delay;
            inputScrol.Value = returned.Speed;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            stopButton.Enabled = true;

            Dictionary<string, int> data = new Dictionary<string, int>();
            for (int i = 0; i < trackedItemsDataGridView.Rows.Count; i++)
            {
                data.Add(trackedItemsDataGridView[0, i].Value.ToString(), Convert.ToInt32(trackedItemsDataGridView[1, i].Value));
            }

            task = new Thread(() =>
            {
                script.Start(data);
            });

            task.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            stopButton.Enabled = false;
            startButton.Enabled = true;
            task.Abort();
        }

        private void scrolDelay_ValueChanged(object sender, EventArgs e)
        {
            if (scrolDelay.Value < scrolDelay.Minimum)
            {
                scrolDelay.Value = scrolDelay.Minimum;
            }
            if (scrolDelay.Value > scrolDelay.Maximum)
            {
                scrolDelay.Value = scrolDelay.Maximum;
            }

            string baseDirectory = Directory.GetCurrentDirectory();
            string filePath = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Randomize.json";
            string json;
            DataRandom random = new DataRandom();
            random.Delay = scrolDelay.Value;
            random.Speed = inputScrol.Value;
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // Красивое форматирование
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Для кириллицы
                };
                json = JsonSerializer.Serialize(random, options);

                if (!File.Exists(baseDirectory))
                    Directory.CreateDirectory(baseDirectory + "\\Data\\");
                if (!File.Exists(baseDirectory + "\\Data\\"))
                    Directory.CreateDirectory(baseDirectory + "\\Data\\Serialize\\");
                File.WriteAllText(filePath, json);
            }
            catch
            {
                MessageBox.Show("Ошибка", "Не удолось сохранить данные.");
                return;
            }
        }

        private void inputScrol_ValueChanged(object sender, EventArgs e)
        {
            if (inputScrol.Value < inputScrol.Minimum)
            {
                inputScrol.Value = inputScrol.Minimum;
            }
            if (inputScrol.Value > inputScrol.Maximum)
            {
                inputScrol.Value = inputScrol.Maximum;
            }

            string baseDirectory = Directory.GetCurrentDirectory();
            string filePath = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Randomize.json";
            string json;
            DataRandom random = new DataRandom();
            random.Delay = scrolDelay.Value;
            random.Speed = inputScrol.Value;
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true, // Красивое форматирование
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Для кириллицы
                };
                json = JsonSerializer.Serialize(random, options);

                if (!File.Exists(baseDirectory))
                    Directory.CreateDirectory(baseDirectory + "\\Data\\");
                if (!File.Exists(baseDirectory + "\\Data\\"))
                    Directory.CreateDirectory(baseDirectory + "\\Data\\Serialize\\");
                File.WriteAllText(filePath, json);
            }
            catch
            {
                MessageBox.Show("Ошибка", "Не удолось сохранить данные.");
                return;
            }
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public string Price { get; set; }
    }
    public class DataRandom
    {
        public decimal Delay {  get; set; }
        public decimal Speed { get; set; }
    }
}