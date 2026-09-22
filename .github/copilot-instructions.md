# Project-wide Copilot instructions

## Project purpose
Build a C# console application for tracking income and expenses as an educational mini project.

## Source of truth
1. Follow docs/project-context.md and docs/acceptance-checklist.md.
2. Preserve required text-based interaction, sorting, filtering, editing, removal, and persistence behavior.
3. When sources conflict, report the conflict before changing code.

## Coding rules
- Use one public class, enum, or interface per file.
- Use ordinary // comments only when they explain why.
- Prefer clear names, short methods, guard clauses, and single responsibility.
- Use decimal for money and represent months consistently with a validated type or value.
- Keep domain logic separate from console input/output and file persistence.
- Make the income-or-expense distinction explicit in the domain model.
- Do not add packages unless the standard library is insufficient and the reason is documented.
- Never hide compilation warnings, persistence failures, or exceptions.

## Workflow rules
- Inspect relevant files before proposing edits.
- Make the smallest coherent change.
- Build and run tests after implementation changes.
- Explain changed files, checks run, remaining risks, and the next recommended task.
- Do not claim a requirement is complete unless it is verified.
