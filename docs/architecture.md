# Architecture — Slice 1: Domain model + in-memory list + display

**Last updated:** 2026-09-23

---

## Design decisions (resolves A1–A7 from acceptance-checklist.md)

| # | Decision | Rationale |
|---|----------|-----------|
| A1 | Month is a plain `int` (1–12) validated at the boundary; no custom type. | Simplest option that rejects invalid values without extra ceremony for a mini project. |
| A2 | Save is triggered explicitly by a menu option ("Save"). | Keeps persistence visible and avoids accidental overwrites during a demo. |
| A3 | JSON file (`moneyitems.json`) in the application's working directory, written with `System.Text.Json`. | JSON is human-readable, round-trips cleanly, and requires no extra packages on .NET 10. |
| A4 | All four fields (title, amount, month, type) are re-prompted on edit; pressing Enter keeps the current value. | Consistent and easy to demo without complex partial-update logic. |
| A5 | Items are selected for edit/remove by the 1-based display index shown in the list. | No stable ID needed for a mini project; index is always visible and unambiguous while the list is on screen. |
| A6 | `ItemType` enum with values `Income` and `Expense`. | Enum is explicit, requires no polymorphism, and satisfies the domain model rule from copilot-instructions.md. |
| A7 | Sorting and filtering are independent operations applied in sequence (filter first, then sort). | Simplest composable behaviour; both apply to the same in-memory list. |

---

## Slice 1 goal

Produce the smallest piece of visible working behavior: a `MoneyItem` domain class, an `ItemType` enum, an `ItemCollection` service, and a `Program.cs` that prints a hard-coded list to the console. No persistence, no editing, no removal.

Acceptance questions answered by this slice: **M1, M2, M3, M5** (partially — display only), **Q1**, **Q8**.

---

## Layer boundaries

```
MoneyTracking/
├── Domain/
│   ├── ItemType.cs          // enum Income | Expense
│   └── MoneyItem.cs         // record: Id, Title, Amount, Month, Type
├── Services/
│   └── ItemCollection.cs    // Add, Remove, GetSorted, GetFiltered
└── Program.cs               // Console loop — reads input, calls services, writes output
```

A separate test project sits beside the application project:

```
MoneyTracking.Tests/
└── ItemCollectionTests.cs   // created in a later slice
```

No class in `Domain/` or `Services/` references `System.Console` or `System.IO`. `Program.cs` owns all console interaction. `JsonPersistence.cs` (added in a later slice) owns all file I/O.

---

## Domain model

### `ItemType` enum — `Domain/ItemType.cs`

```
Income
Expense
```

### `MoneyItem` record — `Domain/MoneyItem.cs`

| Property | Type | Constraint |
|----------|------|------------|
| `Id` | `Guid` | Assigned on construction; never edited |
| `Title` | `string` | Non-empty |
| `Amount` | `decimal` | > 0 |
| `Month` | `int` | 1–12 |
| `Type` | `ItemType` | Income or Expense |

`MoneyItem` is an immutable record. Editing produces a new instance with the same `Id`.

---

## Services

### `ItemCollection` — `Services/ItemCollection.cs`

Holds a `List<MoneyItem>` internally.

| Method | Signature | Notes |
|--------|-----------|-------|
| `Add` | `void Add(MoneyItem item)` | |
| `Remove` | `bool Remove(Guid id)` | Returns false if not found |
| `GetAll` | `IReadOnlyList<MoneyItem> GetAll()` | Unsorted, unfiltered |
| `GetSorted` | `IReadOnlyList<MoneyItem> GetSorted(SortField field, bool ascending)` | |
| `GetFiltered` | `IReadOnlyList<MoneyItem> GetFiltered(ItemType type)` | |
| `Replace` | `void Replace(Guid id, MoneyItem updated)` | Replaces item in place by index |

`SortField` is a second enum (`Month`, `Amount`, `Title`) defined in `Domain/SortField.cs`.

---

## Console interface (Program.cs)

Menu loop — repeats until the user chooses Quit:

```
1. List all
2. Add income
3. Add expense
4. Sort
5. Filter
6. Edit
7. Remove
8. Save
9. Load
0. Quit
```

Input validation: re-prompt on invalid menu choice, non-numeric amount, or out-of-range month. No exception escapes to the top level.

Display format per row: `[index]  [Month]  [Title]  [Amount]  [Income/Expense]`

---

## Persistence (later slice)

`Services/JsonPersistence.cs` exposes two methods:

| Method | Behaviour on failure |
|--------|---------------------|
| `Save(IReadOnlyList<MoneyItem> items, string path)` | Writes atomically; surfaces `IOException` to caller |
| `Load(string path)` | Missing file → returns empty list. Malformed JSON → prints message, returns empty list |

---

## Files to create in Slice 1

| File | Action |
|------|--------|
| `MoneyTracking/Domain/ItemType.cs` | Create |
| `MoneyTracking/Domain/SortField.cs` | Create |
| `MoneyTracking/Domain/MoneyItem.cs` | Create |
| `MoneyTracking/Services/ItemCollection.cs` | Create |
| `MoneyTracking/Program.cs` | Write menu loop with hard-coded seed items for Slice 1 demo |

## Files deferred to later slices

| File | Slice |
|------|-------|
| `MoneyTracking/Services/JsonPersistence.cs` | Slice 2 — Persistence |
| `MoneyTracking.Tests/ItemCollectionTests.cs` | Test slice |

---

## Slice 2 goal — Save and Load (M17–M20, Q6, Q8)

Produce the smallest change that makes **menu options 8 (Save) and 9 (Load)** fully working. No new domain types are needed. Only one new service file is created and `Program.cs` is updated to call it.

Acceptance questions answered by this slice: **M17, M18, M19, M20, Q6** (save/load round-trip test), **Q8** (README updated with file location and format).

### New file — `Services/JsonPersistence.cs`

Namespace: `MoneyTracking.Services`

| Member | Signature | Behaviour |
|--------|-----------|-----------|
| `Save` | `static void Save(IReadOnlyList<MoneyItem> items, string path)` | Serializes to JSON and writes to `path`. Throws `IOException` on disk error (caller prints message). |
| `Load` | `static IReadOnlyList<MoneyItem> Load(string path)` | Missing file → returns empty list (satisfies M19). Malformed JSON → prints a message to `Console.Error` and returns empty list (satisfies M20). |

---

## Slice 6 goal — Export to CSV (new feature)

Produce the smallest change that lets a user export the current item list to a CSV file from the main menu. No new domain types are needed.

Acceptance questions answered by this slice: none currently in the checklist — this is a new capability. A new checklist entry **E1** is recommended after implementation is verified.

### Design

The feature follows the same layer boundary rules as all other slices:

- A new **service** `CsvExport` handles all file I/O and formatting — no `Console` calls inside it.
- `Program.cs` adds one new menu option, prompts for a file path, calls the service, and prints success or error.
- The CSV format is simple: a header row followed by one data row per item, with fields quoted only when they contain a comma.

#### New file — `Services/CsvExport.cs`

Namespace: `MoneyTracking.Services`

| Member | Signature | Behaviour |
|--------|-----------|-----------|
| `Export` | `static void Export(IReadOnlyList<MoneyItem> items, string path)` | Writes UTF-8 CSV to `path`. First row is the header. Throws `IOException` on disk error (caller prints message). |

CSV columns (in order): `Id,Title,Amount,Month,Type`

Field rules:
- `Amount` is written with `F2` and invariant culture (dot decimal separator).
- `Title` is wrapped in double quotes and any embedded double-quote is escaped as `""`.
- All other fields are written as-is (no quoting needed — `Guid`, `int`, and enum names contain no commas or quotes).

#### Changed file — `Program.cs`

- Renumber the existing menu option `7. Discard unsaved changes` → no renumber needed; add `8. Export to CSV` as the new last numbered option before `0. Save and Quit`.
- Add `case "8"` in the switch that calls a new local function `ExportToCsv(collection)`.
- `ExportToCsv` prompts for a file path (non-empty), calls `CsvExport.Export`, prints a confirmation, and catches `IOException` to print an error without crashing.

#### New test — `MoneyTracking.Tests/CsvExportTests.cs`

| Test | What it verifies |
|------|-----------------|
| `Export_WritesHeaderAndOneRow` | A single item produces a correct header and one data row. |
| `Export_EmptyList_WritesHeaderOnly` | An empty collection produces only the header row — no crash. |
| `Export_TitleWithComma_IsQuoted` | A title containing a comma is wrapped in double quotes in the output. |

### Files to create or change in Slice 6

| File | Action |
|------|--------|
| `MoneyTracking/Services/CsvExport.cs` | **Create** — static export service |
| `MoneyTracking/Program.cs` | **Change** — add menu option 8 and `ExportToCsv` local function |
| `MoneyTracking.Tests/CsvExportTests.cs` | **Create** — three deterministic unit tests |
| `docs/acceptance-checklist.md` | **Change** — add row E1 after implementation is verified |

**Serialization details:**
- Uses `System.Text.Json` (already in .NET 10, no extra packages).
- `JsonSerializerOptions` with `WriteIndented = true` for human-readable output.
- The data file is `moneyitems.json` in the application's working directory (documented in README, satisfies M17).
- `MoneyItem` is a record; the serializer handles it automatically via its constructor.

**Atomic write strategy:** Write to a temp file in the same directory, then `File.Move` with `overwrite: true`. This prevents a half-written file if the process is killed mid-save.

### Changes to `Program.cs`

1. Remove seed data (no longer needed once Load works).
2. Auto-load `moneyitems.json` at startup if it exists (silent; missing file is normal first run).
3. Case `"8"` → call `JsonPersistence.Save(collection.GetAll(), DataFile)` and print confirmation.
4. Case `"9"` → call `JsonPersistence.Load(DataFile)`, rebuild collection, print confirmation.
5. Add `const string DataFile = "moneyitems.json";` near the top.

### Changes to `README.md`

Add a "Data file" section stating:
- File name: `moneyitems.json`
- Location: working directory when the app is run (typically the project folder with `dotnet run`).
- Format: JSON array of `MoneyItem` objects.
- The file is created on first explicit Save; a missing file is not an error.

### New test file — `MoneyTracking.Tests/PersistenceTests.cs`

Covers Q6 (save/load round-trip with an isolated temp file):

| Test | Scenario |
|------|----------|
| `SaveAndLoad_RoundTrip` | Save two items, load from same path, assert titles/amounts/types match |
| `Load_MissingFile_ReturnsEmpty` | Load from a non-existent path, assert empty list returned (M19) |
| `Load_MalformedJson_ReturnsEmpty` | Write garbage bytes to a temp file, load, assert empty list returned (M20) |

A test project (`MoneyTracking.Tests/`) must be created if it does not already exist, referencing the main project.

---

## Files to create or change in Slice 2

| File | Action |
|------|--------|
| `MoneyTracking/Services/JsonPersistence.cs` | **Create** |
| `MoneyTracking/Program.cs` | **Edit** — wire Save/Load, remove seed data, add auto-load |
| `README.md` | **Edit** — add Data file section |
| `MoneyTracking.Tests/MoneyTracking.Tests.csproj` | **Create** (if absent) |
| `MoneyTracking.Tests/PersistenceTests.cs` | **Create** |

---

## Handoff

**Summary:** Slice 1 delivered M1–M16, Q1–Q4, Q7. Slice 2 design adds `JsonPersistence.cs` (one static class, two methods), minimal edits to `Program.cs`, a README update, and three persistence tests. No new domain types. Layer boundaries are preserved: `JsonPersistence` touches only `System.IO` and `System.Text.Json`; no console calls inside it.

**Files changed or proposed:** `docs/architecture.md` (this update)

**Verification performed:** Every Slice 2 file maps to at least one mandatory acceptance criterion (M17–M20, Q6, Q8). Design is consistent with decisions A2 (explicit save menu option) and A3 (JSON, working directory).

**Open risks or decisions:**
- Atomic write (temp-file + move) is marginally more complex than a direct write; acceptable given that a half-written file would break M20.
- If the user runs the app from a directory without write permission, Save will throw. A friendly error message in the `catch` block in `Program.cs` mitigates this.

**Recommended next agent:** CSharp Implementer — implement `JsonPersistence.cs`, update `Program.cs`, update `README.md`, create the test project and `PersistenceTests.cs`, build, and run tests to verify M17–M20, Q6, Q8.

---

## Slice 3 goal — Display enrichment: balance, totals, and color (O1, O2, O4)

Produce visible new behavior by enriching the list display and adding a summary line. No structural changes to edit/remove/sort/filter/persistence. All changes are read-only computed values printed to the console.

Acceptance questions answered by this slice: **O1, O2, O4**

### Design decisions for this slice

| Decision | Choice | Reason |
|----------|--------|--------|
| Balance placement | Printed below the item list in `PrintList` | Always visible after listing; no extra menu option needed |
| Totals scope | Total income, total expenses, net balance (income − expenses) | Covers both O1 and O2 in one line |
| Totals by month | Not in this slice — deferred to a future optional slice | Keeps Slice 3 small; by-month totals require grouping logic |
| Color trigger | Income rows green, expense rows red; reset after every row | Per-row reset is safest — no state left behind if printing stops early |
| Color reset safety | `Console.ResetColor()` called in a `finally`-style pattern per row | Prevents terminal color bleed on exceptions |

### Changes to `Program.cs` only

**No new files. No new services. No domain changes.**

#### 1. Colored row printing in `PrintList`

Replace the plain `Console.WriteLine` for each item row with a color-wrapped version:

```
Console.ForegroundColor = item.Type == ItemType.Income
    ? ConsoleColor.Green
    : ConsoleColor.Red;
// print row
Console.ResetColor();
```

The header line, separator, and summary line remain in the default color.

#### 2. Summary line below the list in `PrintList`

After the item rows, print:

```
──────────────────────────────────────────────────────
Income: 3 500,00   Expenses: 1 450,50   Balance: +2 049,50
```

Computed with LINQ over the passed-in `items` list (not `GetAll()` — so it correctly reflects sorted/filtered views):

```csharp
decimal income   = items.Where(i => i.Type == ItemType.Income).Sum(i => i.Amount);
decimal expenses = items.Where(i => i.Type == ItemType.Expense).Sum(i => i.Amount);
decimal balance  = income - expenses;
string sign = balance >= 0 ? "+" : "";
Console.WriteLine($"{"Income:",-12} {income,10:F2}   {"Expenses:",-12} {expenses,10:F2}   Balance: {sign}{balance:F2}");
```

This satisfies O1 (balance) and O2 (totals by type) in a single line that appears consistently after every list operation.

#### 3. No new menu options needed

Because summary appears automatically after every `PrintList` call (menu 1, 4, 5), no extra menu entry is required for O1/O2. This keeps the menu short.

### Layer boundary check

- `PrintList` is a static local function in `Program.cs` — it already owns all console output.
- `ItemCollection`, `MoneyItem`, `JsonPersistence` are untouched.
- `Console.ForegroundColor` and `Console.ResetColor()` live in `Program.cs` — correct layer.

### Edit/remove index stability

`PrintList` is called for display only. The 1-based index is assigned inside the loop (`i + 1`) and is always relative to whichever list is passed in. Color and summary do not affect index assignment. Edit and remove still work correctly.

---

## Files to create or change in Slice 3

| File | Action |
|------|--------|
| `MoneyTracking/Program.cs` | **Edit** — color per row + summary line in `PrintList` |

No other files change.

---

## Handoff

**Summary:** Slice 3 design adds colored income/expense rows and a balance+totals summary line to `PrintList`. All changes are inside one static function in `Program.cs`. No domain, service, test, or persistence files are touched.

**Files changed or proposed:** `docs/architecture.md` (this update)

**Verification performed:** Design checked against O1, O2, O4 acceptance questions and constraints. Color reset per-row eliminates terminal bleed risk. Summary uses the passed-in list, so it reflects filtered/sorted views correctly. Edit/remove index logic is unaffected.

**Open risks or decisions:**
- Terminals that do not support ANSI color (rare on macOS/Windows) will show no color — not a crash, just plain output. Acceptable for a student project.
- `Console.ForegroundColor` is a global process-level setting; if future code adds threading this could race. Not a concern for a single-threaded console app.

**Recommended next agent:** CSharp Implementer — edit `PrintList` in `Program.cs` as described, build, and do a manual visual check to verify green/red rows and the summary line appear correctly.

---

## Slice 4 goal — Keyword search (O3) + two minor fixes

Produce a new menu option that lets the user search items by title keyword. Also fix two open risks from the previous handoff (noted below). All changes are in `Program.cs` and `Services/ItemCollection.cs` only.

Acceptance questions answered by this slice: **O3**

### Design decisions for this slice

| Decision | Choice | Reason |
|----------|--------|--------|
| Menu slot | Option `7` | Free slot; does not conflict with any existing option |
| Search scope | Title only, case-insensitive, substring match | Simplest useful behavior; no new domain types needed |
| Empty search term | Re-prompt until user types something | Consistent with `PromptNonEmpty` pattern already used |
| Zero results | Print "No items match." and return | Same guard as `PrintList` empty-list branch |
| Where search lives | New method `GetByKeyword(string keyword)` in `ItemCollection` | Keeps domain logic out of `Program.cs`; testable |
| Display | Passes result to existing `PrintList` | Reuses color, summary, and column formatting for free |

### New method — `ItemCollection.GetByKeyword`

```csharp
public IReadOnlyList<MoneyItem> GetByKeyword(string keyword) =>
    _items.Where(i => i.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
          .ToList().AsReadOnly();
```

- `OrdinalIgnoreCase` avoids locale-dependent comparison without extra packages.
- Returns `IReadOnlyList` consistent with all other `ItemCollection` query methods.

### Changes to `Program.cs`

1. Add `(7) Search by title keyword` to the menu `WriteLine` block.
2. Add `case "7": SearchItems(collection); break;` to the switch.
3. Add static function `SearchItems`:

```csharp
static void SearchItems(ItemCollection collection)
{
    string keyword = PromptNonEmpty("Search title: ");
    PrintList(collection.GetByKeyword(keyword));
}
```

### Two minor fixes included in this slice

| Risk (from previous handoff) | Fix |
|------------------------------|-----|
| `EditItem` amount field uses system culture `TryParse` | Apply same `NumberStyles.Any, CultureInfo.InvariantCulture` as `PromptDecimal` |
| `AddOrChooseType` silently defaults to Income on invalid input | Change `_` catch-all to a `while` loop that re-prompts — consistent with `PromptItemType` |

### Layer boundary check

- `GetByKeyword` lives in `Services/ItemCollection.cs` — no `Console` reference.
- `SearchItems` lives in `Program.cs` — no domain/service logic.
- `PrintList` is reused unchanged.

---

## Files to create or change in Slice 4

| File | Action |
|------|--------|
| `Services/ItemCollection.cs` | **Edit** — add `GetByKeyword` method |
| `Program.cs` | **Edit** — menu line, switch case, `SearchItems` function, fix Edit amount culture, fix AddOrChooseType loop |

No new files. No domain, persistence, or test project changes.

---

## Handoff

**Summary:** Slice 4 adds keyword search as menu option 7 via a new `GetByKeyword` method in `ItemCollection`. Two minor robustness fixes are bundled (culture on edit amount, re-prompt on invalid add-type). No structural changes to any other feature.

**Files changed or proposed:** `docs/architecture.md` (this update)

**Verification performed:** Design checked against O3 acceptance question and constraint C3 (distinct from type-filter option 5). `GetByKeyword` uses `OrdinalIgnoreCase` — correct for locale-safe title search. `PrintList` reused unchanged.

**Open risks or decisions:** None blocking Slice 4.

**Recommended next agent:** CSharp Implementer — implement `GetByKeyword` in `ItemCollection.cs`, add `SearchItems` and menu wiring in `Program.cs`, apply the two minor fixes, build, verify zero errors.

---

## Slice 5 goal — Month filter added to Filter (option 5)

Extend menu option 5 so the user can optionally narrow results by a specific month in addition to (or instead of) type. Produces visible new behavior with the smallest possible change: one new service method, one extended prompt in `Program.cs`.

Acceptance questions answered by this slice: **M9, M10** (evidence updated — filter now also supports month), **A7** (sort + filter combination: filter-by-month composes with type filter).

### Design decisions for this slice

| Decision | Choice | Reason |
|----------|--------|--------|
| Filter UI | Sequential prompts — type first, then optional month | Simplest; keeps each prompt single-purpose |
| Month prompt | Optional — pressing Enter skips month filtering | Users who only want type-filter are unaffected |
| Month validation | Same 1–12 guard as `PromptMonth`; Enter = no month filter | Consistent with existing validation pattern |
| Service method | New `GetFiltered(ItemType type, int? month)` overload | `null` month means "all months"; composable with existing logic |
| Existing `GetFiltered(ItemType)` | Kept unchanged | All existing callers (`ShowItems`, tests) continue working |

### New service method — `ItemCollection.GetFiltered(ItemType, int?)`

```csharp
public IReadOnlyList<MoneyItem> GetFiltered(ItemType type, int? month) =>
    _items.Where(i => i.Type == type && (month == null || i.Month == month))
          .ToList().AsReadOnly();
```

`month == null` means no month restriction — returns all items of the given type. This overload resolves unambiguously alongside the existing `GetFiltered(ItemType)` overload.

### Changes to `Program.cs`

**Replace `FilterItems`:**

```csharp
static void FilterItems(ItemCollection collection)
{
    ItemType type = PromptItemType();
    int? month = PromptOptionalMonth();
    PrintList(collection.GetFiltered(type, month));
}
```

**New helper `PromptOptionalMonth`:**

```csharp
static int? PromptOptionalMonth()
{
    while (true)
    {
        Console.Write("Month (1-12, or Enter for all months): ");
        string input = Console.ReadLine() ?? "";
        if (input.Length == 0) return null;
        if (int.TryParse(input, out int m) && m >= 1 && m <= 12) return m;
        Console.WriteLine("Month must be 1–12, or press Enter to skip.");
    }
}
```

### Layer boundary check

- New overload in `ItemCollection` — no `Console` or `File` references.
- `PromptOptionalMonth` in `Program.cs` — no domain logic.
- `PrintList` reused unchanged.
- Existing `GetFiltered(ItemType)` overload untouched — `ShowItems` and tests continue passing.

---

## Files to create or change in Slice 5

| File | Action |
|------|--------|
| `Services/ItemCollection.cs` | **Edit** — add `GetFiltered(ItemType type, int? month)` overload |
| `Program.cs` | **Edit** — update `FilterItems`, add `PromptOptionalMonth` |

No new files. No domain, persistence, or test project changes needed (existing filter tests still pass; new overload can optionally be tested).

---

## Handoff

**Summary:** Slice 5 adds an optional month sub-filter to the existing type-filter (option 5). One new service overload, two changed lines in `Program.cs`, one new helper. The original `GetFiltered(ItemType)` overload is untouched so all 30 existing tests continue to pass.

**Files changed or proposed:** `docs/architecture.md` (this update)

**Verification performed:** Design checked against M9/M10 (filter by type still works), A7 (filter + sort compose correctly — month filter applies before sort). No mandatory behavior is changed.

**Open risks or decisions:** None blocking Slice 5.

**Recommended next agent:** CSharp Implementer — add the overload to `ItemCollection.cs`, update `FilterItems` and add `PromptOptionalMonth` in `Program.cs`, build, verify zero errors and all 30 tests still pass.
