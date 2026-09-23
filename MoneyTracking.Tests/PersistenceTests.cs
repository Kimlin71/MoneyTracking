using MoneyTracking.Domain;
using MoneyTracking.Services;
using Xunit;

namespace MoneyTracking.Tests;

// Each test uses a real temporary file so the JSON round-trip is tested end-to-end
// Path.GetTempFileName() creates an empty file in the OS temp folder; finally blocks delete it
public class PersistenceTests
{
    [Fact]
    public void SaveAndLoad_RoundTrip_RestoresAllFields()
    {
        string path = Path.GetTempFileName();
        try
        {
            var items = new List<MoneyItem>
            {
                new(Guid.NewGuid(), "Salary",    3500m, 1, ItemType.Income),
                new(Guid.NewGuid(), "Groceries",  250m, 2, ItemType.Expense),
            };

            JsonPersistence.Save(items, path);
            IReadOnlyList<MoneyItem> loaded = JsonPersistence.Load(path);

            Assert.Equal(2, loaded.Count);
            Assert.Equal("Salary",    loaded[0].Title);
            Assert.Equal(3500m,       loaded[0].Amount);
            Assert.Equal(1,           loaded[0].Month);
            Assert.Equal(ItemType.Income,   loaded[0].Type);
            Assert.Equal("Groceries", loaded[1].Title);
            Assert.Equal(ItemType.Expense,  loaded[1].Type);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsEmptyList()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        IReadOnlyList<MoneyItem> result = JsonPersistence.Load(path);
        Assert.Empty(result);
    }

    [Fact]
    public void Load_MalformedJson_ReturnsEmptyList()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "{ this is not valid json !!!");
            IReadOnlyList<MoneyItem> result = JsonPersistence.Load(path);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // ── SEC-3: MaxDepth guard ─────────────────────────────────────────────────

    [Fact]
    public void Load_DeeplyNestedJson_ReturnsEmptyList()
    {
        // Build JSON that exceeds the MaxDepth = 8 limit
        string path = Path.GetTempFileName();
        try
        {
            string nested = new string('[', 20) + new string(']', 20);
            File.WriteAllText(path, nested);
            IReadOnlyList<MoneyItem> result = JsonPersistence.Load(path);
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // ── SEC-4: no orphaned .tmp file ──────────────────────────────────────────

    [Fact]
    public void Save_WhenMoveSucceeds_NoTmpFileRemains()
    {
        string path = Path.GetTempFileName();
        string tmp  = path + ".tmp";
        try
        {
            var items = new List<MoneyItem>
            {
                new(Guid.NewGuid(), "Test", 1m, 1, ItemType.Income),
            };

            JsonPersistence.Save(items, path);

            Assert.False(File.Exists(tmp));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
