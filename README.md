# Money Tracking Agent Starter

This starter pack adds reusable GitHub Copilot context, custom agents, prompt files, acceptance questions, and workflow documentation to a C# Money Tracking repository.

## Intended destination
/Users/kim/Library/CloudStorage/OneDrive-EQITInnovationsSwedenAB/Desktop/UTV 26/Projects/money-tracking-agent-starter

## Install
Copy the money-tracking-agent-starter folder into the Projects directory above, or copy its contents into the root of an existing C# Money Tracking repository. Preserve the .github folder.

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
