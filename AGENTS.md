# Agent operating guide

This repository uses specialized agents with shared context.

## Shared goal
Deliver a correct, understandable C# console application that satisfies docs/project-context.md and remains suitable for a student presentation.

## Definition of done
A change is done only when:
- The relevant acceptance questions can be answered Yes.
- The solution builds without errors.
- Relevant tests pass.
- The command-line interface remains readable and usable.
- Saved data can be restored when persistence is in scope.
- Documentation reflects the implementation.

## Boundaries
- Do not rewrite the entire solution when a focused change is enough.
- Do not replace student-owned decisions silently.
- Do not fabricate build or test results.
- Use decimal for monetary amounts.
- Preserve the explicit distinction between income and expense.
- Keep generated or test data clearly separated from production data.

## Handoff format
Every agent ends with:
1. Summary
2. Files changed or proposed
3. Verification performed
4. Open risks or decisions
5. Recommended next agent
