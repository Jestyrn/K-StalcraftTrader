namespace TraderForStalCraft
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TabControll = new TabControl();
            MainPage = new TabPage();
            tabPage2 = new TabPage();
            TabControll.SuspendLayout();
            SuspendLayout();
            // 
            // TabControll
            // 
            TabControll.Controls.Add(MainPage);
            TabControll.Controls.Add(tabPage2);
            TabControll.Dock = DockStyle.Fill;
            TabControll.Location = new Point(0, 0);
            TabControll.Name = "TabControll";
            TabControll.SelectedIndex = 0;
            TabControll.Size = new Size(800, 450);
            TabControll.TabIndex = 0;
            // 
            // MainPage
            // 
            MainPage.Location = new Point(4, 24);
            MainPage.Name = "MainPage";
            MainPage.Padding = new Padding(3);
            MainPage.Size = new Size(792, 422);
            MainPage.TabIndex = 0;
            MainPage.Text = "Основное";
            MainPage.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(792, 422);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TabControll);
            Name = "Form1";
            Text = "Form1";
            TabControll.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl TabControll;
        private TabPage MainPage;
        private TabPage tabPage2;
    }
}
