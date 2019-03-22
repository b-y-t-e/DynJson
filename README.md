# [DYNJSON] | Dynamic API (JSON) prototyping | in .NET

## Description
Allows rapid API (JSON) prototyping using languages / technologies:
 + json
 + c#
 + sql
 + [dynlan](https://github.com/b-y-t-e/DynLan)

It is possible to use almost any language / technology compatible with .net environment via a plugin mechanism.

## Examples
 + Simplest API (plain JSON):
```
{
   "IntField" : 1,
   "TextField" : "abc"
}
```

 + JSON + dynamic field (current datetime):
```
{
   "IntField" : 1,
   "DateField" : @(getdatetime())
}
```

 + JSON + parameters:
```
method(param1, param2: string)
{
   "AnyField" : @(param1),
   "TextField" : @(param2)
}
```

 + JSON + parameters + sql:
```
method(personID: int)
{
   {
      sql(select * from person where id = @personID)
   }
}
```

 + JSON + parameters + sql (array or objects):
```
method(filter: string)
{
   [
      {
         sql(select * from person where description like '%' + @filter + '%')
      }
   ]
}
```

 + JSON + parameters + c#:
```
method(text: string)
{
   {
      "textLength" : c#(text.Length),
      "newTextValue" : c#( string newText = "prefix_" + text; return newText; )
   }
}
```

 + JSON + mixed technologies:
```
method(text0: string)
{
   {
      "newCsValue" : c#( text0 + "!" ) as text1,
      "newSqlValue" : sql( select @text1 + '!' ) as text2,
      "newDynlanValue" : @( text2 + '!' )
   }
}
```
