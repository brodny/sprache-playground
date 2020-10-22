# Sprache lessons learned

## NullReferenceException and sensitivity to the order of the fields

When creating [ExpressionParser](ExpressionParser.cs) (based on <https://github.com/sprache/Sprache/blob/master/samples/LinqyCalculator/ExpressionParser.cs>) I rewrote the original class and changed the order of the private fields. This caused the software to fail in runtime. This is because not all fields were initialized when the main field (Lambda) was initialized and caused a lot of null-reference exception errors. This style is extremely sensitive to the static fields initialization order (see e.g. <https://stackoverflow.com/a/3681278>). A **slight change in the order of the fields** (e.g. due to refactoring) can cause the program to fail - and it **_won't be caught until in runtime_**.
Perhaps a fix to this is to use Parse.Ref() every time another subparser is being referenced. This still needs to be checked. Also, there are possible performance implications - need to check that as well.
