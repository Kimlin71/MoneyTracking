using MoneyTracking.Domain;
using MoneyTracking.Services;
using Xunit;

namespace MoneyTracking.Tests;

public class KeywordSearchTests
{
    private static MoneyItem Item(string title, decimal amount = 100m, int month = 1, ItemType type = ItemType.Income) =>
        new(Guid.NewGuid(), title, amount, month, type);

    [Fact]
    public void GetByKeyword_ExactMatch_ReturnsItem()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary"));
        Assert.Single(c.GetByKeyword("Salary"));
    }

    [Fact]
    public void GetByKeyword_CaseInsensitive_ReturnsItem()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary"));
        Assert.Single(c.GetByKeyword("salary"));
        Assert.Single(c.GetByKeyword("SALARY"));
        Assert.Single(c.GetByKeyword("SaLaRy"));
    }

    [Fact]
    public void GetByKeyword_PartialMatch_ReturnsMatchingItems()
    {
        var c = new ItemCollection();
        c.Add(Item("Side project"));
        c.Add(Item("Electricity"));
        c.Add(Item("Salary"));
        var result = c.GetByKeyword("side");
        Assert.Single(result);
        Assert.Equal("Side project", result[0].Title);
    }

    [Fact]
    public void GetByKeyword_NoMatch_ReturnsEmptyList()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary"));
        Assert.Empty(c.GetByKeyword("xyz"));
    }

    [Fact]
    public void GetByKeyword_MultipleMatches_ReturnsAll()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary"));
        c.Add(Item("Salary bonus"));
        c.Add(Item("Rent", type: ItemType.Expense));
        var result = c.GetByKeyword("salary");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetByKeyword_EmptyCollection_ReturnsEmptyList()
    {
        var c = new ItemCollection();
        Assert.Empty(c.GetByKeyword("Salary"));
    }

    // Amount is not a searchable field — search is title-only
    [Fact]
    public void GetByKeyword_AmountAsKeyword_ReturnsEmpty()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary", amount: 32000m));
        Assert.Empty(c.GetByKeyword("32000"));
    }

    [Fact]
    public void GetByKeyword_DoesNotMutateCollection()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary"));
        c.Add(Item("Rent", type: ItemType.Expense));
        c.GetByKeyword("salary");
        Assert.Equal(2, c.GetAll().Count);
    }
}
