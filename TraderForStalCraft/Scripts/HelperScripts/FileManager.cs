using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TraderForStalCraft.Interfaces;
using TraderForStalCraft.Proprties;

namespace TraderForStalCraft.Scripts.HelperScripts
{
    public class FileManager
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public List<Rectangls> LoadFromJson(string path)
        {
            try
            {
                string json = ReadAllText(path);
                return JsonSerializer.Deserialize<List<Rectangls>>(json, _jsonOptions) ??
                       throw new InvalidDataException("Deserialization returned null");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load JSON from {path}", ex);
            }
        }

        public void SaveToJson<T>(string path, T data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, _jsonOptions);
                string directory = Path.GetDirectoryName(path);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save JSON to {path}", ex);
            }
        }

        public List<T> ParseFile<T>(string path) where T : IProduct, new()
        {
            if (!Exists(path))
                throw new FileNotFoundException("File not found", path);

            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".txt" => ParseTextFile<T>(path),
                ".xls" or ".xlsx" => ParseExcelFile<T>(path),
                _ => throw new NotSupportedException($"Format {extension} is not supported")
            };
        }

        private List<T> ParseTextFile<T>(string path) where T : IProduct, new()
        {
            var lines = File.ReadAllLines(path)
                          .Where(line => !string.IsNullOrWhiteSpace(line))
                          .ToList();

            var result = new List<T>();
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    result.Add(new T
                    {
                        Name = parts[0].Trim(),
                        Price = parts[1].Trim()
                    });
                }
            }

            if (result.Count == 0)
                throw new InvalidDataException("File does not contain valid data");

            return result;
        }

        private List<T> ParseExcelFile<T>(string path) where T : IProduct, new()
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = Path.GetExtension(path).ToLower() == ".xlsx"
                    ? new XSSFWorkbook(stream)
                    : new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);
                var result = new List<T>();

                for (int row = 1; row <= sheet.LastRowNum; row++)
                {
                    var currentRow = sheet.GetRow(row);
                    if (currentRow == null) continue;

                    var product = new T
                    {
                        Name = GetCellValue(currentRow.GetCell(0)),
                        Price = GetCellValue(currentRow.GetCell(1))
                    };

                    if (!string.IsNullOrWhiteSpace(product.Name))
                    {
                        result.Add(product);
                    }
                }

                if (result.Count == 0)
                    throw new InvalidDataException("Excel file does not contain valid data");

                return result;
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
    }
}
