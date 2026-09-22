# Money Tracking project context

## Goal
Create a C# console application that lets a user track income and expenses by title, amount, and month.

## Required behavior
### Item model
- Model every item with a title, monetary amount, and month.
- Explicitly distinguish income from expense.
- Use decimal for monetary amounts.

### Collection and display
- Store and display a collection of money items.
- Sort the collection in ascending or descending order.
- Support sorting by month, amount, or title.
- Filter the display to only expenses or only incomes.

### Interaction
- Provide a text-based command-line interface.
- Let users add income and expense items.
- Let users edit existing items.
- Let users remove existing items.
- Let users quit the application.

### Persistence
- Save the current item list to a file.
- Load the saved item list so the previous state is restored after restarting the application.
- Handle a missing or malformed data file explicitly rather than silently losing data.

## Optional features
Examples include totals by month or type, balance calculation, search, validation messages, colored output, CSV export, pagination, and unit tests beyond the mandatory behaviors.

## Design decisions to resolve
- Choose and document whether income/expense is represented by an enum, subclasses, or another explicit model. Prefer the simplest clear design.
- Choose and document the persisted file format and data-file location.
- Define how month input is represented and validated.
- Define how an item is selected for editing or removal, preferably with a stable identifier.

## Output expectations
Display a readable list containing title, amount, month, and income/expense type. Menus and prompts should make add, list, sort, filter, edit, remove, save, and quit actions discoverable.

## Source
Adapted from the supplied Money Tracking Project Specification.
