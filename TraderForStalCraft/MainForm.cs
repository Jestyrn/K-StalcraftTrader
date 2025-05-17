using System.Diagnostics;
using System.Windows.Forms;
using Emgu.CV.Ocl;
using MiNET.Blocks;
using NPOI.SS.Formula.Functions;
using TraderForStalCraft.Interfaces;
using TraderForStalCraft.Proprties;
using TraderForStalCraft.Scripts;
using TraderForStalCraft.Scripts.HelperScripts;
using TraderForStalCraft.Scripts.MainScripts;

namespace TraderForStalCraft
{
    public partial class MainForm : Form
    {
        private StartingScript _runningScript;
        private readonly FileManager _fileManager;
        private readonly string _configPath;
        private readonly string _serializePath;
        private readonly string _serializeHeaderPath;
        private AppConfig _config;
        private string _currentDragText;
        private CancellationTokenSource _cts;
        private readonly Logger _logger = new Logger();
        private const Keys StopKey = Keys.F12;
        private ScreenProcessor screenProcessor;
        private Dictionary<string, Rectangle> _templates;
        private Bitmap screen;

        public MainForm(FileManager fileManager)
        {
            // отредактировать главную таблицу
            // Добавить возможность чистить таблицу
            // Сделать загрузку(только по наличию файла) и очистку главной таблицы SearchItems.lot
            InitializeComponent();

            _fileManager = fileManager;
            _serializeHeaderPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Serialize", "Save");
            _serializePath = Path.Combine(Directory.GetCurrentDirectory(),"Data", "Blueprints");
            _configPath = Path.Combine(_serializeHeaderPath, "Config.json");

            InitializeDragDropSystem();
            InitializeTrackedItemsGrid();
            screenProcessor = new ScreenProcessor(_serializePath);

            LoadConfig();
            SetupEventHandlers();

            KeyboardHook.OnKeyPressed += key =>
            {
                if (key == StopKey && _runningScript?.IsRunning == true)
                    this.Invoke((MethodInvoker)StopScript);
            };

            KeyboardHook.Start();
        }

        private void InitializeDragDropSystem()
        {
            dragDropPanel.AllowDrop = true;
            dragDropInfoLabel.Text = "Перетащите файл сюда (.txt, .xls, .xlsx)";

            // Явная подписка на события
            dragDropPanel.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    dragDropInfoLabel.Text = "Отпустите для загрузки";
                    dragDropInfoLabel.ForeColor = Color.Blue;
                    e.Effect = DragDropEffects.Copy;
                }
            };

            dragDropPanel.DragLeave += (s, e) =>
            {
                dragDropInfoLabel.Text = "Перетащите файл сюда (.txt, .xls, .xlsx)";
                dragDropInfoLabel.ForeColor = SystemColors.ControlText;
            };

            dragDropPanel.DragDrop += (s, e) =>
            {
                try
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length == 1)
                    {
                        ProcessDroppedFile(files[0]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    dragDropInfoLabel.Text = "Перетащите файл сюда (.txt, .xls, .xlsx)";
                    dragDropInfoLabel.ForeColor = SystemColors.ControlText;
                }
            };
        }

        private void InitializeTrackedItemsGrid()
        {
            trackedItemsDataGridView.Columns.Clear();

            // Колонка "Название"
            var nameColumn = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Название",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };

            // Колонка "Цена"
            var priceColumn = new DataGridViewTextBoxColumn
            {
                Name = "Price",
                HeaderText = "Макс. цена",
                DataPropertyName = "Price",
                Width = 150
            };

            trackedItemsDataGridView.Columns.AddRange(nameColumn, priceColumn);
            trackedItemsDataGridView.AllowUserToAddRows = false;
        }

        private void ProcessDroppedFile(string filePath)
        {
            try
            {
                var products = _fileManager.ParseFile<Product>(filePath);

                // Очищаем и обновляем DataGridView
                trackedItemsDataGridView.Rows.Clear();

                foreach (var product in products)
                {
                    trackedItemsDataGridView.Rows.Add(product.Name, product.Price);
                }

                // Сохраняем изменения
                SaveConfig();

                MessageBox.Show($"Успешно загружено {products.Count} предметов",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки файла: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (_fileManager.Exists(_configPath))
                {
                    _config = _fileManager.LoadFromJson<AppConfig>(_configPath);
                    ApplyConfigToUI();
                }
                else
                {
                    _config = new AppConfig();
                    Directory.CreateDirectory(Path.GetDirectoryName(_configPath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки конфига: {ex.Message}");
                _config = new AppConfig();
            }
        }

        private void ApplyConfigToUI()
        {
            scrolDelay.Value = _config.ActionDelay;
            inputScrol.Value = _config.InputSpeed;
            SkipPagesCheckbox.Checked = _config.SkipPages;

            trackedItemsDataGridView.Rows.Clear();
            foreach (var item in _config.TrackedItems)
            {
                trackedItemsDataGridView.Rows.Add(item.Name, item.Price);
            }
        }

        private void SaveConfig()
        {
            _config.ActionDelay = scrolDelay.Value;
            _config.InputSpeed = inputScrol.Value;
            _config.SkipPages = SkipPagesCheckbox.Checked;

            _config.TrackedItems.Clear();
            foreach (DataGridViewRow row in trackedItemsDataGridView.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    _config.TrackedItems.Add(new Product
                    {
                        Name = row.Cells[0].Value.ToString(),
                        Price = row.Cells[1].Value.ToString()
                    });
                }
            }

            _fileManager.SaveToJson(_configPath, _config);
        }

        private void SetupEventHandlers()
        {
            // Обработчики NumericUpDown и CheckBox
            scrolDelay.ValueChanged += (s, e) => SaveConfig();
            inputScrol.ValueChanged += (s, e) => SaveConfig();
            SkipPagesCheckbox.CheckedChanged += (s, e) => SaveConfig();

            // Кнопки управления
            SaveDataButton.Click += (s, e) => SaveConfig();
            DeleteDataButton.Click += DeleteTrackedItems;
            loadItemsButton.Click += LoadItemsFromFile;

            // Drag&Drop обработчики (лямбда-версия)
            dragDropPanel.DragEnter += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    dragDropInfoLabel.Text = "Отпустите файл для загрузки";
                    dragDropInfoLabel.ForeColor = Color.Blue;
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            };

            dragDropPanel.DragLeave += (sender, e) =>
            {
                dragDropInfoLabel.Text = _currentDragText;
                dragDropInfoLabel.ForeColor = SystemColors.ControlText;
            };

            dragDropPanel.DragDrop += (sender, e) =>
            {
                try
                {
                    dragDropInfoLabel.Text = _currentDragText;
                    dragDropInfoLabel.ForeColor = SystemColors.ControlText;

                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files.Length == 1)
                        {
                            LoadFile(files[0]);
                        }
                        else
                        {
                            MessageBox.Show("Пожалуйста, перетащите только один файл",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке файла: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };


            startButton.Click += (s, e) => StartScript();
            stopButton.Click += (s, e) => StopScript();
        }

        private void StartScript()
        {
            Process[] gameProcesses = Process.GetProcessesByName("stalcraft");
            try
            {
                if (gameProcesses.Length < 1 || gameProcesses == null)
                {
                    MessageBox.Show("Игра не запущена, не найден процесс stalcraft");
                }

                if (_runningScript != null && _runningScript.IsRunning)
                {
                    MessageBox.Show("Скрипт уже запущен");
                    return;
                }

                var itemsData = GetTrackedItems();
                if (itemsData.Count == 0)
                {
                    MessageBox.Show("Нет предметов для отслеживания");
                    return;
                }

                startButton.Enabled = false;
                stopButton.Enabled = true;

                CompletePreparation.pathToFile = _serializeHeaderPath;

                _cts = new CancellationTokenSource();
                _runningScript = new StartingScript(scrolDelay.Value, inputScrol.Value, _logger, screenProcessor, _fileManager);

                Task.Run(() => _runningScript.Start(itemsData, _cts.Token, SkipPagesCheckbox.Checked));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void StopScript()
        {
            try
            {
                _cts?.Cancel();
                _runningScript?.Stop();
                startButton.Enabled = true;
                stopButton.Enabled = false;
                MessageBox.Show("Скрипт остановлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private Dictionary<string, int> GetTrackedItems()
        {
            var items = new Dictionary<string, int>();
            foreach (DataGridViewRow row in trackedItemsDataGridView.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    if (int.TryParse(row.Cells[1].Value.ToString(), out int price))
                    {
                        items[row.Cells[0].Value.ToString()] = price;
                    }
                }
            }
            return items;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            _runningScript?.Stop();
            base.OnFormClosing(e);
        }

        private void LoadItemsFromFile(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Excel файлы (*.xls, *.xlsx)|*.xls;*.xlsx";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFile(dialog.FileName);
                }
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                var products = _fileManager.ParseFile<Product>(path);

                // Очищаем только если загрузка прошла успешно
                trackedItemsDataGridView.Rows.Clear();

                foreach (var product in products)
                {
                    if (!string.IsNullOrWhiteSpace(product.Name) && !string.IsNullOrWhiteSpace(product.Price))
                    {
                        trackedItemsDataGridView.Rows.Add(product.Name, product.Price);
                    }
                }

                SaveConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteTrackedItems(object sender, EventArgs e)
        {
            trackedItemsDataGridView.Rows.Clear();
            _config.TrackedItems.Clear();
            _fileManager.SaveToJson(_configPath, _config);
        }

        private void dragDropPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                dragDropInfoLabel.Text = "Отпустите файл для загрузки";
                dragDropInfoLabel.ForeColor = Color.Blue;
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void dragDropPanel_DragLeave(object sender, EventArgs e)
        {
            dragDropInfoLabel.Text = _currentDragText;
            dragDropInfoLabel.ForeColor = SystemColors.ControlText;
        }

        private void dragDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                dragDropInfoLabel.Text = _currentDragText;
                dragDropInfoLabel.ForeColor = SystemColors.ControlText;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length == 1)
                    {
                        LoadFile(files[0]);
                    }
                    else
                    {
                        MessageBox.Show("Пожалуйста, перетащите только один файл",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке файла: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                MessageBox.Show("Название предмета не может быть пустым", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(product.Price, out _))
            {
                MessageBox.Show("Цена должна быть числом", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}