using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TraderForStalCraft.Data.Serialize
{
    internal class Serialize
    {
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
                var returned = JsonSerializer.Deserialize<Dictionary<string, Rectangle>>(json) ?? new Dictionary<string, Rectangle>();
                return returned;
            }
            else
                return null;
        }

        public void SaveData(string path, Dictionary<string, Rectangle> Matches)
        {
            string json;
            string direct = Directory.GetCurrentDirectory();

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            json = JsonSerializer.Serialize(Matches, options);

            if (!File.Exists(path))
            {
                if (!Directory.Exists(direct))
                {
                    throw new Exception("Не удалось найти корневую папку\n" +
                        "переустановите приложение.\n" +
                        $"путь: \"{direct}\"");
                }
                else
                {
                    if (!Directory.Exists(direct + @"\Data"))
                    {
                        Directory.CreateDirectory(direct + "\\Data");
                        direct += @"\Data";
                    }

                    if (!Directory.Exists(direct += @"\Serialize"))
                    {
                        Directory.CreateDirectory(direct + "\\Serialize");
                        direct += @"\Serialize";
                    }

                    File.WriteAllText(path, json);
                }
            }
            else
            {
                File.WriteAllText(path, json);
            }
        }
    }
}
