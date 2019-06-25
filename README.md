# [DYNJSON] | Dynamic API prototyping | JSON | in .NET

## Description
DynJson is a .net library that allows to rapid API (JSON) prototyping using technologies:
 + json
 + [javascript](https://github.com/sebastienros/jint)
 + c#
 + sql
 + [dynlan](https://github.com/b-y-t-e/DynLan)

It is possible to use almost any technology compatible with .net environment via a plugin mechanism.

## Examples
 + Simple API (plain JSON):
```
{
   "IntField" : 1,
   "TextField" : "abc"
}
```
```
{"IntField":1,"TextField":"abc"}
```

 + JSON + javascript code:
```
{
   "IntField" : 1,
   "DateField" : js(Date())
}
```
```
{"IntField":1,"DateField":"Wed Dec 31 1969 16:00:00 GMT-08:00"}
```

 + JSON + parameters:
```
method(param1, param2: string)
{
   "AnyField" : js(param1),
   "TextField" : js(param2)
}
```

 + JSON + parameters + sql:
```
method(personID: int)
{
   {
      query(select * from person where id = @personID)
   }
}
```

 + JSON + parameters + sql (array or objects):
```
method(filter: string)
{
   [
      {
         query(select * from person where description like '%' + @filter + '%')
      }
   ]
}
```

 + JSON + parameters + c#:
```
method(text: string)
{
   {
      "textLength" : cs(text.Length),
      "newTextValue" : cs( string newText = "prefix_" + text; return newText; )
   }
}
```

 + JSON + parameter + c# + sql + js:
```
method(text0: string)
{
   {
      "newCsValue" : cs( text0 + "!" ) as text1,
      "newSqlValue" : query( select @text1 + '!' ) as text2,
      "newDynlanValue" : js( text2 + '!' )
   }
}
```
