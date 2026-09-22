namespace MoneyTracking.Domain;

// A record is immutable by default — editing an item produces a new instance via 'with'
// decimal is used for Amount because float and double lose precision with money
public record MoneyItem(
    Guid Id,      // unique identifier that never changes, even after an edit
    string Title,
    decimal Amount,
    int Month,    // 1–12; validated before construction
    ItemType Type);
