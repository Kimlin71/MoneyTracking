using MoneyTracking.Domain;
using MoneyTracking.Services;
using Xunit;

namespace MoneyTracking.Tests;

public class CsvExportTests
{
    [Fact]
    public void Export_WritesHeaderAndOneRow()
    {
        string path = Path.GetTempFileName();
        try
        {
            var items = new List<MoneyItem>
            {
                new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), "Salary", 3500.50m, 1, ItemType.Income),
            };

            CsvExport.Export(items, path);

            string[] lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Equal("Id,Title,Amount,Month,Type", lines[0]);
            Assert.Equal("aaaaaaaa-0000-0000-0000-000000000001,Salary,3500.50,1,Income", lines[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Export_EmptyList_WritesHeaderOnly()
    {
        string path = Path.GetTempFileName();
        try
        {
            CsvExport.Export(new List<MoneyItem>(), path);

            string[] lines = File.ReadAllLines(path);
            Assert.Single(lines);
            Assert.Equal("Id,Title,Amount,Month,Type", lines[0]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Export_TitleWithComma_IsQuoted()
    {
        string path = Path.GetTempFileName();
        try
        {
            var items = new List<MoneyItem>
            {
                new(Guid.NewGuid(), "Food, drinks", 100m, 3, ItemType.Expense),
            };

            CsvExport.Export(items, path);

            string[] lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Contains("\"Food, drinks\"", lines[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Export_TitleWithQuote_IsEscaped()
    {
        string path = Path.GetTempFileName();
        try
        {
            var items = new List<MoneyItem>
            {
                new(Guid.NewGuid(), "Say \"hello\"", 50m, 1, ItemType.Income),
            };

            CsvExport.Export(items, path);

            string[] lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Contains("\"Say \"\"hello\"\"\"", lines[1]);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
