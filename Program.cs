using MoneyTracking.Domain;
using MoneyTracking.Services;

// ItemCollection holds all items in memory while the program is running
var collection = new ItemCollection();

// Remove seed data once persistence is implemented
collection.Add(new MoneyItem(Guid.NewGuid(), "Salary",      3500.00m, 1, ItemType.Income));
collection.Add(new MoneyItem(Guid.NewGuid(), "Rent",        1200.00m, 1, ItemType.Expense));
collection.Add(new MoneyItem(Guid.NewGuid(), "Freelance",    800.00m, 2, ItemType.Income));
collection.Add(new MoneyItem(Guid.NewGuid(), "Groceries",    250.50m, 2, ItemType.Expense));

// Loop runs until the user chooses Quit (case "0" calls return)
while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Money Tracker ===");
    Console.WriteLine("1. List all");
    Console.WriteLine("2. Add income");
    Console.WriteLine("3. Add expense");
    Console.WriteLine("4. Sort");
    Console.WriteLine("5. Filter");
    Console.WriteLine("6. Edit");
    Console.WriteLine("7. Remove");
    Console.WriteLine("8. Save  (not yet available)");
    Console.WriteLine("9. Load  (not yet available)");
    Console.WriteLine("0. Quit");
    Console.Write("> ");

    // ?? "" means: if ReadLine returns null (e.g. piped input ends), treat it as empty
    string choice = Console.ReadLine() ?? "";

    switch (choice)
    {
        case "1":
            PrintList(collection.GetAll());
            break;

        case "2":
            AddItem(collection, ItemType.Income);
            break;

        case "3":
            AddItem(collection, ItemType.Expense);
            break;

        case "4":
            SortItems(collection);
            break;

        case "5":
            FilterItems(collection);
            break;

        case "6":
            EditItem(collection);
            break;

        case "7":
            RemoveItem(collection);
            break;

        // Both 8 and 9 fall through to the same message because they share behavior
        case "8":
        case "9":
            Console.WriteLine("Persistence is not yet implemented.");
            break;

        // return exits the top-level program, ending the process cleanly
        case "0":
            return;

        default:
            Console.WriteLine("Invalid choice, please try again.");
            break;
    }
}

static void PrintList(IReadOnlyList<MoneyItem> items)
{
    if (items.Count == 0)
    {
        Console.WriteLine("No items.");
        return;
    }
    Console.WriteLine();
    // Negative width left-aligns text; positive width right-aligns (used for Amount)
    Console.WriteLine($"{"#",-4} {"Month",-6} {"Title",-20} {"Amount",10}  {"Type"}");
    Console.WriteLine(new string('-', 54));
    for (int i = 0; i < items.Count; i++)
    {
        MoneyItem item = items[i];
        // i + 1 so the displayed index starts at 1, matching what users type for edit/remove
        Console.WriteLine($"{i + 1,-4} {item.Month,-6} {item.Title,-20} {item.Amount,10:F2}  {item.Type}");
    }
}

static void AddItem(ItemCollection collection, ItemType type)
{
    string title = PromptNonEmpty("Title: ");
    decimal amount = PromptDecimal("Amount: ");
    int month = PromptMonth("Month (1-12): ");
    // Guid.NewGuid() creates a unique ID that will never collide, even across restarts
    collection.Add(new MoneyItem(Guid.NewGuid(), title, amount, month, type));
    Console.WriteLine("Added.");
}

// Asks the user which field and direction to sort by, then prints the sorted result
static void SortItems(ItemCollection collection)
{
    SortField field = PromptSortField();
    bool ascending = PromptAscending();
    PrintList(collection.GetSorted(field, ascending));
}

// Keeps looping until the user enters a valid field choice
static SortField PromptSortField()
{
    while (true)
    {
        Console.WriteLine("Sort by: 1=Month  2=Amount  3=Title");
        switch (Console.ReadLine())
        {
            case "1": return SortField.Month;
            case "2": return SortField.Amount;
            case "3": return SortField.Title;
            default: Console.WriteLine("Invalid choice, enter 1, 2, or 3."); break;
        }
    }
}

// Returns true for ascending, false for descending
static bool PromptAscending()
{
    while (true)
    {
        Console.WriteLine("Direction: 1=Ascending  2=Descending");
        switch (Console.ReadLine())
        {
            case "1": return true;
            case "2": return false;
            default: Console.WriteLine("Invalid choice, enter 1 or 2."); break;
        }
    }
}

static void FilterItems(ItemCollection collection)
{
    ItemType type = PromptItemType();
    PrintList(collection.GetFiltered(type));
}

// Keeps looping until the user enters a valid type choice
static ItemType PromptItemType()
{
    while (true)
    {
        Console.WriteLine("Show: 1=Income  2=Expense");
        switch (Console.ReadLine())
        {
            case "1": return ItemType.Income;
            case "2": return ItemType.Expense;
            default: Console.WriteLine("Invalid choice, enter 1 or 2."); break;
        }
    }
}

static void EditItem(ItemCollection collection)
{
    IReadOnlyList<MoneyItem> all = collection.GetAll();
    if (all.Count == 0) { Console.WriteLine("No items."); return; }
    PrintList(all);

    int index = PromptIndex("Edit #: ", all.Count);
    MoneyItem existing = all[index - 1];

    Console.Write($"Title [{existing.Title}]: ");
    // Keep the existing value if the user just presses Enter (empty input)
    string title = Console.ReadLine() is { Length: > 0 } t ? t : existing.Title;

    Console.Write($"Amount [{existing.Amount:F2}]: ");
    string amountInput = Console.ReadLine() ?? "";
    // Keep the existing value if the input is empty or not a valid positive number
    decimal amount = decimal.TryParse(amountInput, out decimal a) && a > 0 ? a : existing.Amount;

    Console.Write($"Month [{existing.Month}]: ");
    string monthInput = Console.ReadLine() ?? "";
    // Keep the existing value if the input is empty or outside 1-12
    int month = int.TryParse(monthInput, out int m) && m >= 1 && m <= 12 ? m : existing.Month;

    Console.WriteLine($"Type: 1=Income  2=Expense  (current: {existing.Type})");
    // _ is the discard pattern - matches anything not already handled, keeping the existing type
    ItemType type = Console.ReadLine() switch
    {
        "1" => ItemType.Income,
        "2" => ItemType.Expense,
        _   => existing.Type
    };

    // 'with' creates a new record copying all fields, then overrides only the ones listed
    collection.Replace(existing.Id, existing with { Title = title, Amount = amount, Month = month, Type = type });
    Console.WriteLine("Updated.");
}

static void RemoveItem(ItemCollection collection)
{
    IReadOnlyList<MoneyItem> all = collection.GetAll();
    if (all.Count == 0) { Console.WriteLine("No items."); return; }
    PrintList(all);

    int index = PromptIndex("Remove #: ", all.Count);
    // Remove by Id rather than index so a future concurrent edit cannot target the wrong item
    collection.Remove(all[index - 1].Id);
    Console.WriteLine("Removed.");
}

// Repeats the prompt until the user types at least one character
static string PromptNonEmpty(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        string value = Console.ReadLine() ?? "";
        if (value.Length > 0) return value;
        Console.WriteLine("Cannot be empty.");
    }
}

// TryParse returns false instead of throwing an exception on bad input
static decimal PromptDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0) return value;
        Console.WriteLine("Enter a positive number.");
    }
}

// Validates the range here so the domain record is never constructed with an invalid month
static int PromptMonth(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int value) && value >= 1 && value <= 12) return value;
        Console.WriteLine("Month must be 1–12.");
    }
}

// max is passed in so this helper works for any list size without being hardcoded
static int PromptIndex(string prompt, int max)
{
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int value) && value >= 1 && value <= max) return value;
        Console.WriteLine($"Enter a number between 1 and {max}.");
    }
}
