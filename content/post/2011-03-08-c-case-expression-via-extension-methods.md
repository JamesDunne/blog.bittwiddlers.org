---
author: jaymz
categories:
- Uncategorized
date: '2011-03-08T19:36:13'
tags:
- c#
- case
- expression
- linq
- statement
- switch
title: C# Case Expression via Extension Methods
---
As a veteran C# developer yourself, I'm sure you're familiar with the `switch`
statement. Since it is a statement this means you cannot effectively use this
ever-so-useful construct in an expression, such as in a LINQ query. This is a
shame, and it irks me greatly that I have to resort to emulating the switch
behavior with a series of chained ternary operators (a ? b : c ? d : e ? f : g
...) in LINQ. Lucky for you, I am more susceptible to NIH than any man alive.
I felt the need to investigate options in making a functional `case`
expression for C#, or an equivalent cheap look-alike :). LINQ is great at
expressing basic data transformation operations like joins, groupings,
projections, etc., but it's not so great at conditional processing. A foreach
loop with a switch statement would be a better engine for this type of task,
but frankly sometimes it's just too tempting to start off by writing a LINQ
query to get the job done. Using LINQ brings you the benefits of not having to
worry about implementation details while also not increasing your bug surface
area when you implement these basic transformations wrong. Let's look at an
example naive query that needs to do conditional processing:

    
    
    from line in lines
    where line.Length > 0
    let cols = line.Split('\t')
    where cols.Length == 2
    let row = new {
      // 'A' = added, 'D' = deleted, 'M' = modified
      operation = cols[0].Trim(),
      data = cols[1].Trim()
    }
    let added = "added " + row.data + " " + File.ReadAllLines(row.data).Length.ToString()
    let deleted = "deleted " + row.data + " " + File.ReadAllLines(row.data).Length.ToString()
    let modified = "modified " + row.data + " " + File.ReadAllLines(row.data).Length.ToString()
    select (row.operation == "A") ? added
      : (row.operation == "D") ? deleted
      : (row.operation == "M") ? modified
      : String.Empty
    

This works but is wasteful in terms of processing since we're always
generating the `added`, `deleted`, and `modified` strings regardless of the
input condition. The more dependent variables you introduce, the more waste
the query has to generate and select out. What we really want here is a
**switch** statement, but a statement cannot belong in an expression.
Expressions may only be composed of other expressions. Let's see how this
query transforms when I introduce my Case() extension method that I wrote for
[LinqFilter](http://bittwiddlers.org/?p=141 "Blog link"):

    
    
    from line in lines
    where line.Length > 0
    let cols = line.Split('\t')
    where cols.Length == 2
    let row = new {
      // 'A' = added, 'D' = deleted, 'M' = modified
      operation = cols[0].Trim(),
      data = cols[1].Trim()
    }
    select row.operation.Case(
      () => String.Empty,                           // default case
      Match.Case("A", () => "added " + row.data + " " + File.ReadAllLines(row.data).Length.ToString()),
      Match.Case("D", () => "deleted " + row.data + " " + File.ReadAllLines(row.data).Length.ToString()),
      Match.Case("M", () => "modified " + row.data + " " + File.ReadAllLines(row.data).Length.ToString())
    )
    

Through the use of generic extension methods and lambda expressions, we're
able to get a lot of expressibility here. This overload of the Case()
extension method accepts first a default case lambda expression which will
only be invoked when all other cases fail to match the source value, which in
this case is `row.operation`'s value. What follows is a `params CaseMatch<T,
U>[]` which is C# syntactic sugar for writing something along the lines of
`new CaseMatch<T, U>[] { ... }` at the call site. These `CaseMatch<T, U>`s are
small containers that hold the case match value and the lambda expression to
invoke to yield the result of the case expression if the match is made. We use
lambdas so that the expression to return is not evaluated until the match is
made. This prevents unnecessary work from being done or causing side effects.
Think of it as passing in a function to be evaluated rather than hard-coding
an expression in the parameter to be evaluated at the call site of the Case()
extension method. There are two generic arguments used: `T` and `U`. `T`
represents the type you are matching on and `U` represents the type you wish
to define as the result of the Case() method. Just because you are matching on
string values doesn't mean you always want to return a string value from your
case expressions. :) A small static class named `Match` was created which
houses a single static method `Case` in order to shorten the syntax of
creating `CaseMatch<T,U>` instances. Since static methods can use type
inference to automagically determine your `T` and `U` generic types, this
significantly shortens the amount of code you have to write in order to define
cases. Otherwise, you would have to write `new CaseMatch<string, string>("A",
() => "added" + row.data)` each time. Which looks shorter/simpler to you? When
you call the Case() extension method, the `CaseMatch<T,U>` params array is
processed in order and each test value is compared against the source value
which Case() was called on. If there is a match, the method returns the
evaluated lambda for that case. There is no checking for non-unique test
values, so if you repeat a case only the first case will ever receive the
match. It is an O(n) straightforward algorithm and does no precomputation or
table lookups. Another overload of Case() is available for you to provide an
`IEqualityComparer<T>` instance. This is a big win over the **switch**
statement IMO, which to the best of my knowledge does not allow custom
equality comparers to perform the matching logic and is limited to the
behavior set forth by the C# language specification. With this ability, you
could specify `StringComparer.OrdinalIgnoreCase` in order to do case-
insensitive string matching, something not easily/safely done with the switch
statement. The ability to supply a custom IEqualityComparer<T> also opens up
the set of possibilities for doing case matches on non-primitive types, like
custom classes and structs that would not normally be able to be used in a
switch statement. In order to play with this extension method for yourselves,
either [browse my LinqFilter SVN
repository](http://bittwiddlers.org/viewsvn/trunk/public/LinqFilter/LinqFilter.Extensions/?root=WellDunne)
and download the code from the LinqFilter.Extensions project (CaseMatch.cs and
Extensions/CaseExtensions.cs), or download the [LinqFilter
release](http://bittwiddlers.org/viewsvn/trunk/public/LinqFilter/LinqFilter/bin/Release
/LinqFilter-Release.zip?view=co&root=WellDunne "Download ZIP") and play with
it in your LINQ queries.