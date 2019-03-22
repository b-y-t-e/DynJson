# [DYNJSON] | Dynamic API (JSON) | in .NET

## Description
Allows rapid API (JSON) prototyping using languages / technologies:
 + json
 + c#
 + sql
 + [dynlan] (https://github.com/b-y-t-e/DynLan)

It is possible to use almost any language / technology compatible with .net environment via a plugin mechanism.

## Examples

 + Basic usage:
```
// result = 4
Object result = new Compiler().
  Compile(" return 1 + 3 ").
  Eval();
```
