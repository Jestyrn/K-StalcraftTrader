using System.Configuration;

namespace TraderForStalCraft
{
    partial class MainForm : Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            TabControll = new TabControl();
            MainPage = new TabPage();
            startButton = new Button();
            stopButton = new Button();
            itemsDataGridView = new DataGridView();
            balanceLabel = new Label();
            foundItemsLabel = new Label();
            bidsMadeLabel = new Label();
            SettingsPage = new TabPage();
            dragDropPanel = new Panel();
            dragDropInfoLabel = new Label();
            DeleteDataButton = new Button();
            SaveDataButton = new Button();
            loadItemsButton = new Button();
            minDelayInput = new NumericUpDown();
            maxDelayInput = new NumericUpDown();
            label1 = new Label();
            delayRangeLabel = new Label();
            toLabel = new Label();
            trackedItemsLabel = new Label();
            trackedItemsDataGridView = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            toolTip = new ToolTip(components);
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            TabControll.SuspendLayout();
            MainPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)itemsDataGridView).BeginInit();
            SettingsPage.SuspendLayout();
            dragDropPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)minDelayInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)maxDelayInput).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackedItemsDataGridView).BeginInit();
            SuspendLayout();
            // 
            // TabControll
            // 
            TabControll.Controls.Add(MainPage);
            TabControll.Controls.Add(SettingsPage);
            TabControll.Dock = DockStyle.Fill;
            TabControll.Location = new Point(0, 0);
            TabControll.Name = "TabControll";
            TabControll.SelectedIndex = 0;
            TabControll.Size = new Size(900, 526);
            TabControll.TabIndex = 0;
            // 
            // MainPage
            // 
            MainPage.Controls.Add(startButton);
            MainPage.Controls.Add(stopButton);
            MainPage.Controls.Add(itemsDataGridView);
            MainPage.Controls.Add(balanceLabel);
            MainPage.Controls.Add(foundItemsLabel);
            MainPage.Controls.Add(bidsMadeLabel);
            MainPage.Location = new Point(4, 24);
            MainPage.Name = "MainPage";
            MainPage.Padding = new Padding(3);
            MainPage.Size = new Size(892, 498);
            MainPage.TabIndex = 0;
            MainPage.Text = "Основное";
            MainPage.UseVisualStyleBackColor = true;
            // 
            // startButton
            // 
            startButton.Location = new Point(20, 20);
            startButton.Name = "startButton";
            startButton.Size = new Size(100, 30);
            startButton.TabIndex = 0;
            startButton.Text = "Запустить";
            startButton.UseVisualStyleBackColor = true;
            // 
            // stopButton
            // 
            stopButton.Enabled = false;
            stopButton.Location = new Point(130, 20);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(100, 30);
            stopButton.TabIndex = 1;
            stopButton.Text = "Остановить";
            stopButton.UseVisualStyleBackColor = true;
            // 
            // itemsDataGridView
            // 
            itemsDataGridView.AllowDrop = true;
            itemsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            itemsDataGridView.BackgroundColor = Color.White;
            itemsDataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.LightGray;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            itemsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            itemsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            itemsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            itemsDataGridView.EnableHeadersVisualStyles = false;
            itemsDataGridView.Location = new Point(20, 70);
            itemsDataGridView.Name = "itemsDataGridView";
            itemsDataGridView.ReadOnly = true;
            itemsDataGridView.Size = new Size(840, 400);
            itemsDataGridView.TabIndex = 2;
            // 
            // balanceLabel
            // 
            balanceLabel.AutoSize = true;
            balanceLabel.Location = new Point(20, 480);
            balanceLabel.Name = "balanceLabel";
            balanceLabel.Size = new Size(67, 15);
            balanceLabel.TabIndex = 3;
            balanceLabel.Text = "Баланс: 0 ₽";
            // 
            // foundItemsLabel
            // 
            foundItemsLabel.AutoSize = true;
            foundItemsLabel.Location = new Point(361, 480);
            foundItemsLabel.Name = "foundItemsLabel";
            foundItemsLabel.Size = new Size(129, 15);
            foundItemsLabel.TabIndex = 4;
            foundItemsLabel.Text = "Найдено предметов: 0";
            // 
            // bidsMadeLabel
            // 
            bidsMadeLabel.AutoSize = true;
            bidsMadeLabel.Location = new Point(755, 480);
            bidsMadeLabel.Name = "bidsMadeLabel";
            bidsMadeLabel.Size = new Size(105, 15);
            bidsMadeLabel.TabIndex = 5;
            bidsMadeLabel.Text = "Сделано ставок: 0";
            // 
            // SettingsPage
            // 
            SettingsPage.Controls.Add(dragDropPanel);
            SettingsPage.Controls.Add(DeleteDataButton);
            SettingsPage.Controls.Add(SaveDataButton);
            SettingsPage.Controls.Add(loadItemsButton);
            SettingsPage.Controls.Add(minDelayInput);
            SettingsPage.Controls.Add(maxDelayInput);
            SettingsPage.Controls.Add(label1);
            SettingsPage.Controls.Add(delayRangeLabel);
            SettingsPage.Controls.Add(toLabel);
            SettingsPage.Controls.Add(trackedItemsLabel);
            SettingsPage.Controls.Add(trackedItemsDataGridView);
            SettingsPage.Location = new Point(4, 24);
            SettingsPage.Name = "SettingsPage";
            SettingsPage.Padding = new Padding(3);
            SettingsPage.Size = new Size(892, 498);
            SettingsPage.TabIndex = 1;
            SettingsPage.Text = "Настройки";
            SettingsPage.UseVisualStyleBackColor = true;
            // 
            // dragDropPanel
            // 
            dragDropPanel.AllowDrop = true;
            dragDropPanel.BorderStyle = BorderStyle.FixedSingle;
            dragDropPanel.Controls.Add(dragDropInfoLabel);
            dragDropPanel.Location = new Point(20, 317);
            dragDropPanel.Name = "dragDropPanel";
            dragDropPanel.Size = new Size(300, 150);
            dragDropPanel.TabIndex = 6;
            // 
            // dragDropInfoLabel
            // 
            dragDropInfoLabel.AllowDrop = true;
            dragDropInfoLabel.Dock = DockStyle.Fill;
            dragDropInfoLabel.Location = new Point(0, 0);
            dragDropInfoLabel.Name = "dragDropInfoLabel";
            dragDropInfoLabel.Size = new Size(298, 148);
            dragDropInfoLabel.TabIndex = 0;
            dragDropInfoLabel.Text = "Перетащите файл с предметами сюда\r\n(Поддерживаются: .txt, .xls, .xlsx, .xltx, .csv)";
            dragDropInfoLabel.TextAlign = ContentAlignment.MiddleCenter;
            dragDropInfoLabel.DragDrop += dragDropInfoLabel_DragDrop;
            dragDropInfoLabel.DragEnter += dragDropInfoLabel_DragEnter;
            dragDropInfoLabel.DragLeave += dragDropInfoLabel_DragLeave;
            // 
            // DeleteDataButton
            // 
            DeleteDataButton.Location = new Point(625, 437);
            DeleteDataButton.Name = "DeleteDataButton";
            DeleteDataButton.Size = new Size(225, 30);
            DeleteDataButton.TabIndex = 0;
            DeleteDataButton.Text = "Очистить таблицу";
            DeleteDataButton.UseVisualStyleBackColor = true;
            DeleteDataButton.Click += DeleteDataButton_Click;
            // 
            // SaveDataButton
            // 
            SaveDataButton.Location = new Point(350, 437);
            SaveDataButton.Name = "SaveDataButton";
            SaveDataButton.Size = new Size(225, 30);
            SaveDataButton.TabIndex = 0;
            SaveDataButton.Text = "Сохранить таблицу";
            SaveDataButton.UseVisualStyleBackColor = true;
            SaveDataButton.Click += SaveDataButton_Click;
            // 
            // loadItemsButton
            // 
            loadItemsButton.Location = new Point(12, 20);
            loadItemsButton.Name = "loadItemsButton";
            loadItemsButton.Size = new Size(209, 30);
            loadItemsButton.TabIndex = 0;
            loadItemsButton.Text = "Загрузить файл вручную";
            loadItemsButton.UseVisualStyleBackColor = true;
            loadItemsButton.Click += loadItemsButton_Click;
            // 
            // minDelayInput
            // 
            minDelayInput.Location = new Point(21, 273);
            minDelayInput.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            minDelayInput.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            minDelayInput.Name = "minDelayInput";
            minDelayInput.Size = new Size(100, 23);
            minDelayInput.TabIndex = 2;
            toolTip.SetToolTip(minDelayInput, "Минимальная задержка (мс)");
            minDelayInput.Value = new decimal(new int[] { 500, 0, 0, 0 });
            minDelayInput.ValueChanged += minDelayInput_ValueChanged;
            // 
            // maxDelayInput
            // 
            maxDelayInput.Location = new Point(219, 273);
            maxDelayInput.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            maxDelayInput.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            maxDelayInput.Name = "maxDelayInput";
            maxDelayInput.Size = new Size(100, 23);
            maxDelayInput.TabIndex = 3;
            toolTip.SetToolTip(maxDelayInput, "Максимальная задержка (мс)");
            maxDelayInput.Value = new decimal(new int[] { 1500, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 255);
            label1.Name = "label1";
            label1.Size = new Size(142, 15);
            label1.TabIndex = 4;
            label1.Text = "Задержка действий (мс):";
            // 
            // delayRangeLabel
            // 
            delayRangeLabel.AutoSize = true;
            delayRangeLabel.Location = new Point(12, 66);
            delayRangeLabel.Name = "delayRangeLabel";
            delayRangeLabel.Size = new Size(295, 135);
            delayRangeLabel.TabIndex = 4;
            delayRangeLabel.Text = resources.GetString("delayRangeLabel.Text");
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Location = new Point(219, 255);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(20, 15);
            toLabel.TabIndex = 5;
            toLabel.Text = "до";
            // 
            // trackedItemsLabel
            // 
            trackedItemsLabel.AutoSize = true;
            trackedItemsLabel.Location = new Point(350, 20);
            trackedItemsLabel.Name = "trackedItemsLabel";
            trackedItemsLabel.Size = new Size(159, 15);
            trackedItemsLabel.TabIndex = 7;
            trackedItemsLabel.Text = "Отслеживаемые предметы:";
            // 
            // trackedItemsDataGridView
            // 
            trackedItemsDataGridView.AllowUserToAddRows = false;
            trackedItemsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            trackedItemsDataGridView.BackgroundColor = Color.White;
            trackedItemsDataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.LightGray;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            trackedItemsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            trackedItemsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            trackedItemsDataGridView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3 });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Window;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            trackedItemsDataGridView.DefaultCellStyle = dataGridViewCellStyle4;
            trackedItemsDataGridView.EnableHeadersVisualStyles = false;
            trackedItemsDataGridView.Location = new Point(350, 38);
            trackedItemsDataGridView.Name = "trackedItemsDataGridView";
            trackedItemsDataGridView.ReadOnly = true;
            trackedItemsDataGridView.Size = new Size(500, 382);
            trackedItemsDataGridView.TabIndex = 8;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Название";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Макс. цена";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Приоритет";
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 526);
            Controls.Add(TabControll);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            Text = "Stalcraft Trader Bot";
            TabControll.ResumeLayout(false);
            MainPage.ResumeLayout(false);
            MainPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)itemsDataGridView).EndInit();
            SettingsPage.ResumeLayout(false);
            SettingsPage.PerformLayout();
            dragDropPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)minDelayInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)maxDelayInput).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackedItemsDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl TabControll;
        private System.Windows.Forms.TabPage MainPage;
        private System.Windows.Forms.TabPage SettingsPage;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.DataGridView itemsDataGridView;
        private System.Windows.Forms.Label balanceLabel;
        private System.Windows.Forms.Label foundItemsLabel;
        private System.Windows.Forms.Label bidsMadeLabel;
        private System.Windows.Forms.Button loadItemsButton;
        private System.Windows.Forms.NumericUpDown minDelayInput;
        private System.Windows.Forms.NumericUpDown maxDelayInput;
        private System.Windows.Forms.Label delayRangeLabel;
        private System.Windows.Forms.Label toLabel;
        private System.Windows.Forms.Panel dragDropPanel;
        private System.Windows.Forms.Label dragDropInfoLabel;
        private System.Windows.Forms.ToolTip toolTip;
        private DataGridView trackedItemsDataGridView;
        private Label trackedItemsLabel;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private Label label1;
        private Button DeleteDataButton;
        private Button SaveDataButton;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}