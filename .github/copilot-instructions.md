copilot-instructions.md

Purpose

This document captures recommended best practices for using GitHub Copilot CLI (the Copilot terminal assistant) and working with generated code in the ParserObjects repository.

Generated on: 2025-12-06T14:12:08.417Z

Scope

Covers prompting guidelines, safe and minimal code-edit practices, testing and validation steps, security considerations, and contributor etiquette when using the Copilot CLI in this repository.

Best practices

1. Start with context
- Provide a short description of the goal and any constraints (target framework/version, performance, API compatibility). Include the path to the file(s) to change and a short code excerpt when possible.

2. Use explicit, small requests
- Ask for minimal, well-scoped changes (one responsibility per request). Prefer "make the smallest possible change to fix X" instead of broad refactors.

3. Provide expected outputs and tests
- When requesting new features or bug fixes, include desired behavior and tests (unit or integration) to validate the change. If adding behavior, request tests be added alongside implementation.

4. Follow repository rules for edits
- Keep changes minimal and surgical; do not modify unrelated files. Ensure the project builds and existing tests pass for changes that affect runtime behavior.

5. Run documentation and build checks
- After code changes, run the repository's build and test commands. Document any design decisions in docs/ or ARCHITECTURE.md when relevant.

6. Security and secrets
- Never include secrets, credentials, or private keys in prompts or code. Scan generated code for accidental secrets before committing.

7. Commit and review
- Use clear commit messages describing the why and what. Prefer small commits. Always review generated code manually before merging and ensure tests cover new behavior.

8. Use the Copilot CLI features correctly
- When asking about Copilot CLI capabilities or usage, call fetch_copilot_cli_documentation first (it returns authoritative usage and examples). Provide the returned documentation context when following up.
- Group related operations together when invoking tools (e.g., searches, file edits, and tests) to reduce iterative back-and-forth.

Prompting examples

- Fix a failing test:
  "Test X in ParserObjects.Tests\SomeTest.cs is failing with an NRE at line 42. Minimize the change to fix the null check and add a unit test that reproduces the failure. Ensure all tests pass."

- Add a small feature:
  "Add an optional parameter to ParserObjects\Parser.cs::Parse that enables verbose diagnostics. Update callers and add unit tests demonstrating the verbose output but keep API changes minimal."

Tool usage etiquette

- Request one change per prompt when possible; if multiple unrelated changes are required, break them into separate requests.
- If a suggested change is risky or large, ask for a design proposal first rather than an immediate implementation.

Troubleshooting and validation

- If builds or tests fail after a generated change, include build/test logs and request targeted fixes; avoid broad "fix all errors" prompts.
- Use CI and local test runs to validate behavior before asking for further changes.

Contact and escalation

- If uncertain about a design decision or a risky refactor, open an issue or discuss with maintainers before merging large generated changes.

Appendix: Local commands (examples)

- Build solution (Windows):
  dotnet build ParserObjects.sln

- Run tests (Windows):
  dotnet test ParserObjects.Tests\ParserObjects.Tests.csproj

- Run the Copilot CLI's fetch documentation step (in prompts):
  Always call the fetch_copilot_cli_documentation tool first when asking about the CLI's capabilities or help text.

If anything in this document should be adjusted for repository-specific workflows (branch naming, commit hooks, or CI requirements), indicate the desired changes and a maintainer will update this file.