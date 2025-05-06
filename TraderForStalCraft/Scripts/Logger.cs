using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Cmp;
using TraderForStalCraft.Interfaces;

namespace TraderForStalCraft.Scripts
{
    public class Logger
    {
        private readonly string loggerMain;
        private readonly string loggerData;
        private readonly string loggerLogs;
        private readonly string loggerFull;

        public Logger()
        {
            loggerMain = Directory.GetCurrentDirectory();
            loggerData = Path.Combine(loggerMain, "Data");
            loggerLogs = Path.Combine(loggerData, "Logs");

            if (!Directory.Exists(loggerLogs))
                CreateNeedDirectory(loggerLogs);

            loggerFull = Path.Combine(loggerLogs, $"{DateTime.Now.ToString("dd.MM.yy")}.log");
        }

        private void CreateNeedDirectory(string path)
        {
            string currentStepCreate = "";

            try
            {
                if (!Directory.Exists(Path.Combine(loggerMain, loggerData)))
                {
                    currentStepCreate = Path.Combine(loggerMain, loggerData);
                    Directory.CreateDirectory(Path.Combine(loggerMain, loggerData));
                }

                if (!Directory.Exists(Path.Combine(currentStepCreate, loggerLogs)))
                {
                    currentStepCreate = Path.Combine(currentStepCreate, loggerLogs);
                    Directory.CreateDirectory(Path.Combine(currentStepCreate, loggerLogs));
                }
            }
            catch
            {
                MessageBox.Show("Не удалось создать папку для логгирования\n" +
                    $"Ошибка на шаге: {currentStepCreate}",
                    "Ошибка!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        public void Info(string message)
        {
            message = $"[{DateTime.Now:HH:mm:ss}] - [INFO] - {message}\n";

            try
            {
                File.AppendAllText(loggerFull, message);
            }
            catch
            {
                MessageBox.Show("Не удалось записать логи, ниже строка которую не удалось записать\n\n" +
                    $"{message}",
                    "Ошибка!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void Debug(string message)
        {
            message = $"[{DateTime.Now:HH:mm:ss}] - [DEBUG] - {message}\n";

            try
            {
                File.AppendAllText(loggerFull, message);
            }
            catch
            {
                MessageBox.Show("Не удалось записать логи, ниже строка которую не удалось записать\n\n" +
                    $"{message}",
                    "Ошибка!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        public void Warn(string message)
        {
            message = $"[{DateTime.Now:HH:mm:ss}] - [WARN] - {message}\n";

            try
            {
                File.AppendAllText(loggerFull, message);
            }
            catch
            {
                MessageBox.Show("Не удалось записать логи, ниже строка которую не удалось записать\n\n" +
                    $"{message}",
                    "Ошибка!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
