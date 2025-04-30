using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TraderForStalCraft.Data.Serialize
{
    internal class Serialize
    {
        string loggerPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private string _path;

        public Serialize(string path)
        {
            _path = path;
        }

        public Dictionary<string, Rectangle> LoadData()
        {
            if (File.Exists(_path))
            {
                string json = File.ReadAllText(_path);
                Logger("файл прочтен");
                var returned = JsonSerializer.Deserialize<Dictionary<string, Rectangle>>(json) ?? new Dictionary<string, Rectangle>();
                Logger("файл записан в переменную");
                return returned;
            }
            else
                return null;
        }

        public void SaveData(string path, Dictionary<string, Rectangle> Matches)
        {
            string json;
            string direct = Directory.GetCurrentDirectory();

            Logger("Serialize: получены переменные");

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            Logger("Serialize: введены настройки");

            json = JsonSerializer.Serialize(Matches, options);

            Logger("Serialize: файл сериализован");

            if (!File.Exists(path))
            {
                Logger("Serialize: начало сохранения файла");
                if (!Directory.Exists(direct))
                {
                    Logger("Serialize: ошибка - директории не существует");
                    throw new Exception("Не удалось найти корневую папку\n" +
                        "переустановите приложение.\n" +
                        $"путь: \"{direct}\"");
                }
                else
                {
                    if (!Directory.Exists(direct + @"\Data"))
                    {
                        Logger("Serialize: создана папка \\Data");
                        Directory.CreateDirectory(direct + "\\Data");
                        direct += @"\Data";
                    }

                    if (!Directory.Exists(direct += @"\Serialize"))
                    {
                        Logger("Serialize: Создана папка \\Serialize");
                        Directory.CreateDirectory(direct + "\\Serialize");
                        direct += @"\Serialize";
                    }

                    path = direct + "\\" + "PointsSer.json";
                    try
                    {
                        File.WriteAllText(path, json);
                    }
                    catch (Exception ex)
                    {
                        Logger(ex.Message);
                        throw;
                    }
                    Logger("Serialize: данные записаны(2)");
                }
            }
            else
            {
                Logger("Serialize: данные записаны(1)");
                File.WriteAllText(path, json);
            }
        }

        public void Logger(string text)
        {
            text = text + "\n";
            string path = loggerPath + @"\logs.txt";
            if (!File.Exists(path))
                File.WriteAllText(path, text + "\n");
            else
            {
                text += File.ReadAllText(path);
                File.WriteAllText(path, text);
            }
        }
    }
}
