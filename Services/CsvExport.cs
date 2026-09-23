using System.Globalization;
using MoneyTracking.Domain;

namespace MoneyTracking.Services;

public static class CsvExport
{
    // Returns false when fullPath escapes outside safeDir — used by Program.cs and tests
    public static bool IsPathSafe(string fullPath, string safeDir) =>
        fullPath.StartsWith(safeDir + Path.DirectorySeparatorChar, StringComparison.Ordinal)
        || fullPath == safeDir;

    public static void Export(IReadOnlyList<MoneyItem> items, string path)
    {
        using var writer = new StreamWriter(path, append: false, System.Text.Encoding.UTF8);
        writer.WriteLine("Id,Title,Amount,Month,Type");
        foreach (MoneyItem item in items)
        {
            string title = EscapeField(item.Title);
            string amount = item.Amount.ToString("F2", CultureInfo.InvariantCulture);
            writer.WriteLine($"{item.Id},{title},{amount},{item.Month},{item.Type}");
        }
    }

    // Wraps the value in double quotes and escapes any embedded double quotes as ""
    private static string EscapeField(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
            return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
