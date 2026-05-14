# miniscript based on gdscript development roadmap

## high priority

- [x] add support for logical operators (and, or, not)
- [x] implement arrays/lists with index access
- [x] implement dictionaries (key-value pairs)
- [x] add for loops for iterating over collections
- [x] add garbage collection for memory management
- [x] add try/catch for error handling
- [x] add finally and throw for exception handling
- [ ] port all the standard library functions from global to do notations
- [x] implement compound assignment operators (+=, -=, \*=, /=, %=)

## cool features to add

- [x] string interpolation ("hello {name}")
- [x] standard library for math and string manipulation
- [x] improve standard library to be like functions instead of a weird low level thing (string['trim'](text) -> string.trim(text))
- [x] implement SetExpr for dot notation next, to treat dictionaries like objects
- [x] importing other .ms files to share code
- [ ] implement injection of libraries into the interpreter by other projects to add raylib support
- [ ] basic class support for object-oriented programming
- [ ] a bytecode compiler and virtual machine for better performance (or not, idk, not now)

## polish and maintenance

- [x] improve error messages with visual pointers (^) to the source code
- [ ] add unit tests for the lexer and parser
- [ ] document the c# architecture for maintaining the codebase
