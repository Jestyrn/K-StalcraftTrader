using System.ComponentModel;
using System.Text.Json;
using OfficeOpenXml;
using TraderForStalCraft.Scripts;

namespace TraderForStalCraft
{
    public partial class MainForm : Form
    {
        private string mind = "";

        public MainForm()
        {
            string filePath1 = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Serialize.json";
            string filePath2 = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Randomize.json";

            InitializeComponent();

            if (File.Exists(filePath1))
                LoadFromJsonDataGrid(filePath1);
            if (File.Exists(filePath2))
                LoadFromJsonRandomize(filePath2);
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
                                Proirity = temp[2]
                            });
                        }
                        break;
                    default:
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
                trackedItemsDataGridView.Rows.Add(products[i].Name, products[i].Price, products[i].Proirity);
            }
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
                    Proirity = trackedItemsDataGridView[2, i].Value.ToString(),
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
                trackedItemsDataGridView.Rows.Add(returned[i].Name, returned[i].Price, returned[i].Proirity);
            }
        }

        private void DeleteDataButton_Click(object sender, EventArgs e)
        {
            string filePath = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Serialize.json";
            if (File.Exists(filePath))
                File.Delete(filePath);
            trackedItemsDataGridView.Rows.Clear();
        }

        private void minDelayInput_ValueChanged(object sender, EventArgs e)
        {
            string baseDirectory = Directory.GetCurrentDirectory();
            string filePath = Directory.GetCurrentDirectory() + "\\Data\\Serialize\\Randomize.json";
            string json;
            DataRandom random = new DataRandom();
            random.min = minDelayInput.Value;
            random.max = maxDelayInput.Value;
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

        private void LoadFromJsonRandomize(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var returned = JsonSerializer.Deserialize<DataRandom>(json) ?? new DataRandom();

            minDelayInput.Value = returned.min;
            maxDelayInput.Value = returned.max;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            StartingScript.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            StartingScript.Stop();
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Proirity { get; set; }
    }
    public class DataRandom
    {
        public decimal min {  get; set; }
        public decimal max { get; set; }
    }
}