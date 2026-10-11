using BookStoreCursov.Models;
using BookStoreCursov.Services;

public interface IExcelService
{
    byte[] ExportToExcel<T>(List<T> data, string sheetName, Dictionary<string, Func<T, string>> customFields = null);
    byte[] ExportToCsv<T>(List<T> data, Dictionary<string, Func<T, string>> customFields = null);
    Task<ImportResult> ImportFromExcelAsync<T>(
        Stream fileStream,
        BookStoreCursContext context,
        Func<Dictionary<string, object>, int, Task<(bool isValid, string error, T entity)>> validateAndCreate) where T : class;
}