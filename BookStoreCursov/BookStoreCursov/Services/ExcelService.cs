using BookStoreCursov.Models;
using OfficeOpenXml;
using System.Reflection;
using System.Text;

namespace BookStoreCursov.Services
{
    public class ExcelService : IExcelService
    {
        public ExcelService()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Книжный магазин - Курсовая работа");
        }

        public byte[] ExportToExcel<T>(List<T> data, string sheetName, Dictionary<string, Func<T, string>> customFields = null)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            if (data == null || data.Count == 0)
            {
                worksheet.Cells["A1"].Value = "Нет данных";
                return package.GetAsByteArray();
            }

            var columns = GetExportColumns<T>(customFields);

            for (int i = 0; i < columns.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = columns[i].Header;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                for (int col = 0; col < columns.Count; col++)
                {
                    var value = columns[col].ValueGetter(item);
                    worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportToCsv<T>(List<T> data, Dictionary<string, Func<T, string>> customFields = null)
        {
            if (data == null || data.Count == 0)
                return new byte[0];

            var columns = GetExportColumns<T>(customFields);

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            var headers = string.Join(",", columns.Select(c => $"\"{c.Header}\""));
            writer.WriteLine(headers);

            foreach (var item in data)
            {
                var row = string.Join(",", columns.Select(c =>
                {
                    var value = c.ValueGetter(item) ?? "";
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                }));
                writer.WriteLine(row);
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        private List<ExportColumn<T>> GetExportColumns<T>(Dictionary<string, Func<T, string>> customFields)
        {
            var columns = new List<ExportColumn<T>>();

            if (customFields != null)
            {
                foreach (var field in customFields)
                {
                    columns.Add(new ExportColumn<T> { Header = field.Key, ValueGetter = field.Value });
                }
            }
            else
            {
                var properties = typeof(T).GetProperties()
                    .Where(p => !p.Name.EndsWith("Id") && p.Name != "Id" + typeof(T).Name)
                    .ToList();

                foreach (var prop in properties)
                {
                    columns.Add(new ExportColumn<T>
                    {
                        Header = GetHeaderName(prop.Name),
                        ValueGetter = (item) => FormatValue(prop.GetValue(item))
                    });
                }
            }

            return columns;
        }

        private string FormatValue(object value)
        {
            if (value == null) return "";

            if (value is DateOnly dateOnly)
                return dateOnly.ToString("yyyy-MM-dd");
            if (value is DateTime dateTime)
                return dateTime.ToString("yyyy-MM-dd");

            if (value is decimal dec)
                return dec.ToString("F2");

            return value.ToString();
        }

        public async Task<ImportResult> ImportFromExcelAsync<T>(
            Stream fileStream,
            BookStoreCursContext context,
            Func<Dictionary<string, object>, int, Task<(bool isValid, string error, T entity)>> validateAndCreate) where T : class
        {
            var result = new ImportResult();
            var importedData = ReadExcelData(fileStream);

            if (importedData.Count == 0)
            {
                result.Errors.Add("Файл не содержит данных");
                return result;
            }

            for (int i = 0; i < importedData.Count; i++)
            {
                var row = importedData[i];
                var rowNumber = i + 2;

                try
                {
                    var (isValid, error, entity) = await validateAndCreate(row, rowNumber);

                    if (!isValid)
                    {
                        result.Skipped++;
                        if (!string.IsNullOrEmpty(error))
                            result.Errors.Add(error);
                        continue;
                    }

                    context.Set<T>().Add(entity);
                    result.Added++;
                }
                catch (Exception ex)
                {
                    result.Skipped++;
                    result.Errors.Add($"Строка {rowNumber}: {ex.Message}");
                }
            }

            await context.SaveChangesAsync();
            return result;
        }

        private List<Dictionary<string, object>> ReadExcelData(Stream fileStream)
        {
            var result = new List<Dictionary<string, object>>();

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet.Dimension == null)
                return result;

            int colCount = worksheet.Dimension.Columns;
            int rowCount = worksheet.Dimension.Rows;

            if (rowCount < 2)
                return result;

            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Text;
                headers.Add(string.IsNullOrEmpty(header) ? $"Column{col}" : header);
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, object>();
                for (int col = 1; col <= colCount; col++)
                {
                    rowData[headers[col - 1]] = worksheet.Cells[row, col].Text;
                }
                result.Add(rowData);
            }

            return result;
        }

        private string GetHeaderName(string propertyName)
        {
            return propertyName switch
            {
                "IdGenre" => "ID",
                "Title" => "Название",
                "GenreDescription" => "Описание",
                "IdAuthor" => "ID",
                "AuthorName" => "Имя",
                "AuthorSurname" => "Фамилия",
                "AuthorPatronymic" => "Отчество",
                "DateOfBirth" => "Дата рождения",
                "Biography" => "Биография",
                "IdPublishingHouse" => "ID",
                "PublishingHouseAddress" => "Адрес",
                "Phone" => "Телефон",
                "Email" => "Email",
                "IdRole" => "ID",
                "IdOrderStatus" => "ID",
                "IdUser" => "ID",
                "UserLogin" => "Логин",
                "UsersName" => "Имя",
                "UsersSurname" => "Фамилия",
                "UsersPatronymic" => "Отчество",
                "AvatarPath" => "Аватар",
                "IdBook" => "ID",
                "Isbn" => "ISBN",
                "YearOfPublication" => "Год издания",
                "Price" => "Цена",
                "Quantity" => "Количество",
                "BookDescription" => "Описание",
                "CoverPath" => "Обложка",
                "AddDate" => "Дата добавления",
                "IdReview" => "ID",
                "Rating" => "Оценка",
                "TextReview" => "Текст отзыва",
                "IdCart" => "ID",
                "IdOrder" => "ID",
                "DeliveryAddress" => "Адрес доставки",
                "PaymentMethod" => "Способ оплаты",
                "TotalSum" => "Итоговая сумма",
                "IdOrderPosition" => "ID",
                "PurchasePrice" => "Цена покупки",
                "IdNotification" => "ID",
                "NotificationText" => "Текст уведомления",
                "StatusN" => "Прочитано",
                _ => propertyName
            };
        }
    }

    public class ExportColumn<T>
    {
        public string Header { get; set; }
        public Func<T, string> ValueGetter { get; set; }
    }

    public class ImportResult
    {
        public int Added { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}