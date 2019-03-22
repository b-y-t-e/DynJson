# [DYNJSON] | Dynamic API (JSON) | in .NET

## Description
Allows rapid API (JSON) prototyping using languages / technologies:
 + json
 + c#
 + sql
 + [dynlan] (https://github.com/b-y-t-e/DynLan)

It is possible to use almost any language / technology compatible with .net environment via a plugin mechanism.

## Examples
 + Simplest API (plain JSON):
```
{
   "IntField" : 1,
   "TextField" : "abc"
}
```

 + JSON + dynamic datetime value:
```
{
   "IntField" : 1,
   "DateField" : @(getdate())
}
```
