using MoneyTracking.Domain;
using MoneyTracking.Services;
using Xunit;

namespace MoneyTracking.Tests;

// Each export test writes to a real temp file and reads it back; finally blocks clean up the file
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

    // ── IsPathSafe (SEC-2 path traversal guard) ──────────────────────────────

    [Fact]
    public void IsPathSafe_PathInsideBaseDir_ReturnsTrue()
    {
        string baseDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        string fullPath = Path.Combine(baseDir, "export.csv");
        Assert.True(CsvExport.IsPathSafe(fullPath, baseDir));
    }

    [Fact]
    public void IsPathSafe_PathTraversal_ReturnsFalse()
    {
        string baseDir = Path.Combine(Path.GetTempPath(), "safe");
        // Resolve a traversal attempt one level above the safe dir
        string fullPath = Path.GetFullPath(Path.Combine(baseDir, "..", "outside.csv"));
        Assert.False(CsvExport.IsPathSafe(fullPath, baseDir));
    }

    [Fact]
    public void IsPathSafe_ExactlyBaseDir_ReturnsTrue()
    {
        string baseDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        Assert.True(CsvExport.IsPathSafe(baseDir, baseDir));
    }

    [Fact]
    public void IsPathSafe_AbsolutePathOutsideDir_ReturnsFalse()
    {
        string baseDir = Path.Combine(Path.GetTempPath(), "project");
        string fullPath = Path.Combine(Path.GetTempPath(), "other", "file.csv");
        Assert.False(CsvExport.IsPathSafe(fullPath, baseDir));
    }
}
