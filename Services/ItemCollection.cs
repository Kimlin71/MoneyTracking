using MoneyTracking.Domain;

namespace MoneyTracking.Services;

// Owns all in-memory list operations; callers own their own I/O
public class ItemCollection
{
    // readonly means _items cannot be replaced with a different list after construction
    private readonly List<MoneyItem> _items = [];

    public void Add(MoneyItem item) => _items.Add(item);

    public bool Remove(Guid id)
    {
        // FindIndex searches by Id so the correct item is targeted regardless of its position
        int index = _items.FindIndex(i => i.Id == id);
        // index -1 means the Id was not found; return false so the caller knows nothing changed
        if (index < 0) return false;
        _items.RemoveAt(index);
        return true;
    }

    // Replaces the item at the found position; does nothing if the Id is not in the list
    public void Replace(Guid id, MoneyItem updated)
    {
        int index = _items.FindIndex(i => i.Id == id);
        if (index >= 0) _items[index] = updated;
    }

    // AsReadOnly prevents callers from casting back to List and bypassing this class
    public IReadOnlyList<MoneyItem> GetAll() => _items.AsReadOnly();

    public IReadOnlyList<MoneyItem> GetSorted(SortField field, bool ascending)
    {
        // switch expression maps each field to the correct LINQ sort direction
        IEnumerable<MoneyItem> ordered = field switch
        {
            SortField.Month  => ascending ? _items.OrderBy(i => i.Month)  : _items.OrderByDescending(i => i.Month),
            SortField.Amount => ascending ? _items.OrderBy(i => i.Amount) : _items.OrderByDescending(i => i.Amount),
            SortField.Title  => ascending ? _items.OrderBy(i => i.Title)  : _items.OrderByDescending(i => i.Title),
            _ => _items
        };
        return ordered.ToList().AsReadOnly();
    }

    // Where is LINQ for filtering — it keeps only items where the condition is true
    public IReadOnlyList<MoneyItem> GetFiltered(ItemType type) =>
        _items.Where(i => i.Type == type).ToList().AsReadOnly();

    // null month means no month restriction — returns all items of the given type
    public IReadOnlyList<MoneyItem> GetFiltered(ItemType type, int? month) =>
        _items.Where(i => i.Type == type && (month == null || i.Month == month))
              .ToList().AsReadOnly();

    // OrdinalIgnoreCase means "Salary" and "salary" both match the keyword
    public IReadOnlyList<MoneyItem> GetByKeyword(string keyword) =>
        _items.Where(i => i.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
              .ToList().AsReadOnly();
}
