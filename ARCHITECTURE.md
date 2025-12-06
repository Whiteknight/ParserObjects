ARCHITECTURE

Overview

ParserObjects is a .NET library that provides building blocks for implementing parsers using composable parser combinators and reusable parser primitives. It is designed to make it straightforward to define grammars in code, compose small parsers into larger ones, produce structured results (ASTs or domain objects), and support performant parsing strategies (controlled backtracking, memoization, and error reporting). The project also contains supporting tools for benchmarking and code generation.

Design goals

- Composability: small parsers combine into larger ones via combinators (sequence, choice, repetition, optional, etc.).
- Clarity: declarative grammar expression in code with readable error messages.
- Performance: minimize unnecessary allocations, allow memoization and efficient backtracking.
- Testability: clear separation of pure parsing logic and side effects, with comprehensive tests.

High-level architecture

- Core parser primitives: low-level parser implementations that consume input and return parse results (success/failure, value, remaining input, and diagnostics).
- Combinators: higher-order functions that combine primitives into more complex parsers (sequence, choice, many, skip, map, etc.).
- AST / result builders: utilities that transform parser outputs into domain objects or syntax trees.
- Error handling and diagnostics: structured errors and expected-token reporting to improve developer feedback.
- Tooling: code generation and benchmarks live in separate projects to keep core runtime small.

Code map

- ParserObjects\
  The main library containing core parser types, combinators, result types, and any runtime utilities. This is the heart of the project.

- ParserObjects.Tests\
  Unit and integration tests for the library; exercises combinators, error cases, and expected behaviors.

- ParserObjects.CodeGen\
  Tools for generating parser or AST helper code from higher-level specifications; intended to speed development for repetitive grammar patterns.

- ParserObjects.BenchmarkHarness\
  Benchmarking utilities and harness code used to measure performance characteristics of parsers and combinators.

- ParserObjects.BenchmarkConsole\
  Console apps and runnable benchmarks for local performance experiments.

- docs\
  Documentation and user-facing guides; extend with design notes, examples, and migration guides.

- Scripts\
  Build, publish, and utility scripts used by maintainers (CI helpers, packaging, etc.).

Glossary

- Parser: A function or object that consumes input and returns a parse result (success or failure, plus value/state).
- Parser combinator: A higher-order construct that combines parsers to form more complex parsers (e.g., Sequence, Choice, Many).
- Terminal: A parser that matches raw input (characters, tokens) and does not delegate further to other parsers.
- Nonterminal: A parser composed from other parsers (one of the results of applying combinators).
- AST (Abstract Syntax Tree): Structured representation of parsed input used for further processing or code generation.
- Backtracking: The process of reverting input position and parser state when a parsing path fails and trying alternatives.
- Memoization: Caching parse results for given input positions to avoid exponential re-computation in recursive grammars (packrat parsing).
- Lookahead: Inspecting input ahead without consuming it to make parsing decisions.
- Failure / Expectation: Structured error information indicating what token/construct was expected and where the parse failed.

Guidance for contributors

- Keep combinators small and orthogonal: one responsibility per combinator.
- Write tests for both success and failure paths; error reporting is as important as success semantics.
- Use the CodeGen project for repetitive patterns but keep generated code reviewable.

If more detailed module-level diagrams or per-class descriptions are needed, indicate which areas to expand first (core combinators, error model, or codegen).