# Architecture — Slice 1: Domain model + in-memory list + display

**Last updated:** 2026-09-21

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
| `MoneyTracking/Services/JsonPersistence.cs` | Persistence slice |
| `MoneyTracking.Tests/ItemCollectionTests.cs` | Test slice |

---

## Handoff

**Summary:** Resolved all seven ambiguities from the acceptance checklist. Designed a two-layer domain + service structure with a thin `Program.cs` console loop. Slice 1 covers M1–M3, M5 (display), Q1, and Q8.

**Files changed or proposed:** `docs/architecture.md` (created)

**Verification performed:** Design cross-checked against every mandatory criterion in acceptance-checklist.md and the coding rules in copilot-instructions.md (one public type per file, decimal for money, domain separate from I/O).

**Open risks or decisions:** None blocking Slice 1. Persistence error UX (A2 save trigger) is documented and deferred.

**Recommended next agent:** CSharp Implementer — create the five files listed under "Files to create in Slice 1", build, and verify M1–M3, M5, Q1, Q8.
