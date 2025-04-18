using System.ComponentModel;
using System.Text.Json;
using OfficeOpenXml;

namespace TraderForStalCraft
{
    public partial class MainForm : Form
    {
        private string mind = "";

        public MainForm()
        {
            InitializeComponent();
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
    }

    public class Product
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Proirity { get; set; }
    }
}