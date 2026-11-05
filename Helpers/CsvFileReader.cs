using System.Text;
using MiniDpp.Exceptions;
using MiniDpp.Models;

namespace MiniDpp.Helpers
{
    // CSV metnini satırlara ayırır ve başlıkları standart adlara çevirir.
    public static class CsvFileReader
    {
        // Standart sütun adı -> kabul edilen başlık yazımları
        public static readonly Dictionary<string, string[]> ColumnAliases = new()
        {
            ["gtin"] = new[] { "gtin", "gtin14", "gtin_14" },
            ["urun_adi"] = new[] { "urun_adi", "urun", "product_name", "ad" },
            ["model_adi"] = new[] { "model_adi", "model", "model_name", "model_ad" },
            ["batch_no"] = new[] { "batch_no", "batch", "batch_numarasi", "batch_lot", "lot", "lot_no", "parti_no" },
            ["uretim_yeri"] = new[] { "uretim_yeri", "production_place", "uretim_tesisi", "uretim_lokasyonu" },
            ["elyaf_pamuk_yuzde"] = new[] { "elyaf_pamuk_yuzde" },
            ["elyaf_elastan_yuzde"] = new[] { "elyaf_elastan_yuzde" },
            ["karbon_kg_co2e"] = new[] { "karbon_kg_co2e" },
        };

        private static readonly string[] OptionalColumns = { "urun_adi" };

        // Önce UTF-8 (BOM dahil), olmazsa Türkçe Windows kodlaması (1254) denenir.
        public static string Decode(byte[] data)
        {
            string text;
            try
            {
                text = new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(data);
            }
            catch (DecoderFallbackException)
            {
                text = Encoding.GetEncoding(1254).GetString(data);
            }
            return text.TrimStart('﻿');
        }

        // "Model Adı" -> "model_adi"
        public static string NormalizeHeader(string text)
        {
            var builder = new StringBuilder();
            foreach (var character in text.Trim().TrimStart('﻿'))
            {
                builder.Append(character switch
                {
                    'ç' or 'Ç' => 'c',
                    'ğ' or 'Ğ' => 'g',
                    'ı' or 'İ' => 'i',
                    'ö' or 'Ö' => 'o',
                    'ş' or 'Ş' => 's',
                    'ü' or 'Ü' => 'u',
                    ' ' or '-' or '/' or '.' => '_',
                    _ => char.ToLowerInvariant(character)
                });
            }
            return builder.ToString();
        }

        // CSV'yi okur. Satır numarası dosyadaki satırdır; başlık 1. satırdır.
        public static List<CsvRow> ReadRows(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new CsvFormatException("CSV dosyası boş.");
            }

            var firstLine = text.Split('\n')[0];
            var delimiter = firstLine.Count(c => c == ';') > firstLine.Count(c => c == ',') ? ';' : ',';
            var rawRows = Split(text, delimiter);

            var headers = rawRows[0].Cells;
            var positions = FindColumns(headers);

            var rows = new List<CsvRow>();
            foreach (var (line, cells) in rawRows.Skip(1))
            {
                if (cells.All(string.IsNullOrWhiteSpace))
                {
                    continue; // tamamen boş satırlar atlanır
                }

                if (cells.Count != headers.Count)
                {
                    rows.Add(new CsvRow
                    {
                        Line = line,
                        RowError = $"Sütun sayısı başlıkla uyuşmuyor (beklenen: {headers.Count}, gelen: {cells.Count})."
                    });
                    continue;
                }

                rows.Add(new CsvRow
                {
                    Line = line,
                    Values = positions.ToDictionary(p => p.Key, p => cells[p.Value])
                });
            }
            return rows;
        }

        // Standart sütun adı -> CSV içindeki sütun sırası
        private static Dictionary<string, int> FindColumns(List<string> headers)
        {
            var normalized = headers.Select(NormalizeHeader).ToList();
            var positions = new Dictionary<string, int>();

            foreach (var (name, aliases) in ColumnAliases)
            {
                var alias = aliases.FirstOrDefault(normalized.Contains);
                if (alias != null)
                {
                    positions[name] = normalized.IndexOf(alias);
                }
            }

            var missing = ColumnAliases.Keys
                .Where(name => !OptionalColumns.Contains(name) && !positions.ContainsKey(name))
                .ToList();
            if (missing.Count > 0)
            {
                throw new CsvFormatException(
                    "CSV başlığında zorunlu sütunlar eksik: " + string.Join(", ", missing)
                    + ". Bulunan başlıklar: " + string.Join(", ", headers));
            }

            return positions;
        }

        // Tırnak içindeki ayraçları ve satır sonlarını doğru işleyen basit CSV ayrıştırıcı.
        private static List<(int Line, List<string> Cells)> Split(string text, char delimiter)
        {
            var rows = new List<(int, List<string>)>();
            var cells = new List<string>();
            var cell = new StringBuilder();
            var inQuotes = false;
            var line = 1;
            var rowStartLine = 1;

            void EndRow()
            {
                cells.Add(cell.ToString());
                rows.Add((rowStartLine, cells));
                cells = new List<string>();
                cell.Clear();
            }

            for (var i = 0; i < text.Length; i++)
            {
                var character = text[i];

                if (inQuotes)
                {
                    if (character == '"' && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else if (character == '"')
                    {
                        inQuotes = false;
                    }
                    else
                    {
                        if (character == '\n') line++;
                        cell.Append(character);
                    }
                    continue;
                }

                switch (character)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        EndRow();
                        line++;
                        rowStartLine = line;
                        break;
                    default:
                        if (character == delimiter)
                        {
                            cells.Add(cell.ToString());
                            cell.Clear();
                        }
                        else
                        {
                            cell.Append(character);
                        }
                        break;
                }
            }

            if (cell.Length > 0 || cells.Count > 0)
            {
                EndRow();
            }
            return rows;
        }
    }
}
