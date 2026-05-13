# miniscript

miniscript is a lightweight, indentation-based scripting language inspired by gdscript, built using c# and .net 8.

## features

- indentation-based block syntax (like python/gdscript)
- dynamic typing (numbers, strings, booleans, null)
- first-class functions and closures
- while loops and if/else statements
- built-in functions like `print()`, `input()` and `clock()`

## architecture

the project is divided into modular components:

- **lexer**: converts raw text into tokens while tracking indentation levels.
- **parser**: a recursive descent parser that builds an abstract syntax tree (ast).
- **ast**: immutable nodes with c# records.
- **runtime**: a tree-walking interpreter that manages scope and execution.

## getting started

1. ensure you have the .net 8 sdk installed.
2. clone the repo and run the repl:
   `dotnet run`
3. or run a specific script:
   `dotnet run -- example.ms`

## example syntax

```python
func greet(name):
    print("hello " + name)

var count = 0
while count < 3:
    greet("developer")
    count = count + 1
```

## license

this project is mit licensed, open-source and intended for educational purposes.
