# Money Tracking

A C# console application for tracking income and expenses by title, amount, and month.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run
```

The app starts with a numbered menu. Type the number and press Enter to choose an option.

## Run tests

```bash
dotnet test MoneyTracking.Tests/MoneyTracking.Tests.csproj
```

## Data file

| Property | Value |
|----------|-------|
| File name | `moneyitems.json` |
| Location | Working directory when the app is run (typically the project folder with `dotnet run`) |
| Format | JSON array of money-item objects |
| Created by | Menu option **8. Save** — the file is not created automatically |
| Missing file | Not an error; the app starts with an empty list on first run |

## Recommended sequence
1. Run 01-analyze-requirements with Requirements Analyst.
2. Resolve ambiguities in the acceptance checklist.
3. Run 02-design-slice with CSharp Architect.
4. Run 03-implement-slice with CSharp Implementer.
5. Run the Test Designer for behavior introduced by the slice.
6. Run 04-review-slice with Code Reviewer.
7. Correct findings with CSharp Implementer.
8. Run 05-document-slice with Project Documenter.
9. Commit the verified slice in Git.

## First suggested slice
Create the solution, a money-item model with title, amount, month, and income/expense type, plus an in-memory collection. Add a text-based command that prints all items. Do not introduce editing, removal, persistence, or optional features in the first slice.
