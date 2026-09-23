using MoneyTracking.Domain;
using MoneyTracking.Services;
using Xunit;

namespace MoneyTracking.Tests;

public class ItemCollectionTests
{
    // Helper so each test can build items without repeating the full constructor
    private static MoneyItem Item(string title, decimal amount, int month, ItemType type = ItemType.Income) =>
        new(Guid.NewGuid(), title, amount, month, type);

    // ── GetSorted ────────────────────────────────────────────────────────────

    [Fact]
    public void GetSorted_ByMonth_Ascending_ReturnsMonthsInOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("C", 10m, 12));
        c.Add(Item("A", 20m,  1));
        c.Add(Item("B", 15m,  6));

        var result = c.GetSorted(SortField.Month, ascending: true);

        Assert.Equal(new[] { 1, 6, 12 }, result.Select(i => i.Month));
    }

    [Fact]
    public void GetSorted_ByMonth_Descending_ReturnsMonthsInReverseOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("C", 10m, 12));
        c.Add(Item("A", 20m,  1));
        c.Add(Item("B", 15m,  6));

        var result = c.GetSorted(SortField.Month, ascending: false);

        Assert.Equal(new[] { 12, 6, 1 }, result.Select(i => i.Month));
    }

    [Fact]
    public void GetSorted_ByAmount_Ascending_ReturnsAmountsInOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("A", 300m, 1));
        c.Add(Item("B", 100m, 2));
        c.Add(Item("C", 200m, 3));

        var result = c.GetSorted(SortField.Amount, ascending: true);

        Assert.Equal(new[] { 100m, 200m, 300m }, result.Select(i => i.Amount));
    }

    [Fact]
    public void GetSorted_ByAmount_Descending_ReturnsAmountsInReverseOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("A", 300m, 1));
        c.Add(Item("B", 100m, 2));
        c.Add(Item("C", 200m, 3));

        var result = c.GetSorted(SortField.Amount, ascending: false);

        Assert.Equal(new[] { 300m, 200m, 100m }, result.Select(i => i.Amount));
    }

    [Fact]
    public void GetSorted_ByTitle_Ascending_ReturnsAlphabeticalOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("Zebra",  10m, 1));
        c.Add(Item("Apple",  20m, 2));
        c.Add(Item("Mango",  30m, 3));

        var result = c.GetSorted(SortField.Title, ascending: true);

        Assert.Equal(new[] { "Apple", "Mango", "Zebra" }, result.Select(i => i.Title));
    }

    [Fact]
    public void GetSorted_ByTitle_Descending_ReturnsReverseAlphabeticalOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("Zebra",  10m, 1));
        c.Add(Item("Apple",  20m, 2));
        c.Add(Item("Mango",  30m, 3));

        var result = c.GetSorted(SortField.Title, ascending: false);

        Assert.Equal(new[] { "Zebra", "Mango", "Apple" }, result.Select(i => i.Title));
    }

    // GetSorted must not mutate the original insertion order
    [Fact]
    public void GetSorted_DoesNotChangeOriginalOrder()
    {
        var c = new ItemCollection();
        c.Add(Item("Z", 10m, 12));
        c.Add(Item("A", 20m,  1));

        c.GetSorted(SortField.Month, ascending: true);

        Assert.Equal("Z", c.GetAll()[0].Title);
    }

    // ── GetFiltered ──────────────────────────────────────────────────────────

    [Fact]
    public void GetFiltered_Income_ReturnsOnlyIncomeItems()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary", 500m, 1, ItemType.Income));
        c.Add(Item("Rent",   400m, 1, ItemType.Expense));
        c.Add(Item("Bonus",  100m, 2, ItemType.Income));

        var result = c.GetFiltered(ItemType.Income);

        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(ItemType.Income, i.Type));
    }

    [Fact]
    public void GetFiltered_Expense_ReturnsOnlyExpenseItems()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary", 500m, 1, ItemType.Income));
        c.Add(Item("Rent",   400m, 1, ItemType.Expense));
        c.Add(Item("Food",    80m, 2, ItemType.Expense));

        var result = c.GetFiltered(ItemType.Expense);

        Assert.Equal(2, result.Count);
        Assert.All(result, i => Assert.Equal(ItemType.Expense, i.Type));
    }

    [Fact]
    public void GetFiltered_NoMatch_ReturnsEmptyList()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary", 500m, 1, ItemType.Income));

        Assert.Empty(c.GetFiltered(ItemType.Expense));
    }

    // ── GetFiltered(type, month) — Slice 5 overload ──────────────────────────

    [Fact]
    public void GetFiltered_ByTypeAndMonth_ReturnsMatchingItems()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary",    500m, 1, ItemType.Income));
        c.Add(Item("Freelance", 200m, 2, ItemType.Income));
        c.Add(Item("Rent",      400m, 1, ItemType.Expense));

        var result = c.GetFiltered(ItemType.Income, month: 1);

        Assert.Single(result);
        Assert.Equal("Salary", result[0].Title);
    }

    [Fact]
    public void GetFiltered_ByTypeAndNullMonth_ReturnsSameAsTypeOnlyOverload()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary",    500m, 1, ItemType.Income));
        c.Add(Item("Freelance", 200m, 2, ItemType.Income));
        c.Add(Item("Rent",      400m, 1, ItemType.Expense));

        // null month means no month restriction — result must match the single-argument overload
        Assert.Equal(
            c.GetFiltered(ItemType.Income).Select(i => i.Id),
            c.GetFiltered(ItemType.Income, month: null).Select(i => i.Id));
    }

    [Fact]
    public void GetFiltered_ByTypeAndMonth_NoMatch_ReturnsEmptyList()
    {
        var c = new ItemCollection();
        c.Add(Item("Salary", 500m, 1, ItemType.Income));

        Assert.Empty(c.GetFiltered(ItemType.Income, month: 6));
    }
}
