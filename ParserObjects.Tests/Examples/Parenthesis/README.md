# Parenthesis Validator

This example solves the parenthesis validation problem where `'('`, `')'`, `'['`, `']'`, `'{'` and `'}'` in a string must be properly ordered and balanced. Every opening character must be properly matched with the correct closing character.

The parser is recursive with `Deferred()` and shows how `Rule()` can be used to match things in specific orders to progress.