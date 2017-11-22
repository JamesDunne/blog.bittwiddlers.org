---
author: jaymz
categories:
- Uncategorized
date: '2013-08-16T23:58:56'
tags: []
title: MiniLISP for C#
---
[MiniLISP](https://github.com/JamesDunne/mini-lisp) is an extremely minimal
implementation of a limited yet powerful enough dialect of LISP which I
invented for use in C# and .NET applications. It is a dynamically yet strongly
typed language with very few primitives: function invocation `(func param1
param2)`, lists `[a b c]`, identifiers `hello`, integers `1024`, and quoted
strings `'single quotes with \\ backslash \n escaping and multi-line
literals.'`. It is implemented across two C# source files and relies on no
external dependencies other than the .NET framework, thus making it ideal for
direct inclusion into any existing project.

Function invocation (denoted with parentheses or curly braces) and lists
(denoted with square brackets) are kept as separate primitives to allow easy
distinguishing of data from code, both visually and as an implementation
simplification. This may be against common LISP idioms, but I find it both
practical and useful for the simplicity of the language. This dialect is
intended to make it extremely simple for C# developers to implement external
functions for the LISP code to call out to.

Only integers and strings are currently supported as the primitive data types.
There is no support for `float`, `double`, or `decimal` types. This will
likely be revised in the future.

The reason for the choice of single-quoted strings (as opposed to the more
popular double-quoted strings) is so that the language may be embedded in a C#
string literal with minimal fuss. C# string literals are denoted with double
quotes and one would have to double-up each double quote in order to escape
it. Using single quote characters allows us to avoid this nastiness. Also, we
gain free backslash escape sequences for our strings since C# raw string
literals (e.g. `@"raw \ string"`) do not interpret backslash escape sequences.

Some example MiniLISP code:

    
    
    (prefix st [StudentID FirstName LastName])
    {prefix st [StudentID FirstName LastName]}
    

Both of these expressions are identical. Function invocation is denoted with
either parentheses or curly braces. Curly braces are used to allow embedding
of MiniLISP code inside SQL query text, for example. Standard SQL syntax makes
little if no use of curly brace characters, so they are an ideal signal to
indicate the start and end of a section of MiniLISP code. Java's JDBC escape
syntax demonstrates success with their use of curly braces to escape out of
SQL.

Function parameters are separated by whitespace, as are list items.

Identifiers and quoted strings are both treated as strings for data purposes.
Identifiers may contain alphanumeric sequences and the hyphen character, but
must start with either a hyphen or an alpha character.

Quoted strings must start and end with a single quote character and may
contain common backslash escape sequences.

Integers must start with a numeric character and proceed in kind.

To parse the above MiniLISP fragment in C#:

    
    
    const string code = @"{prefix st [StudentID FirstName LastName]}";
    var lex = new Lexer(new StringReader(f));
    var prs = new Parser(lex);
    var expr = prs.ParseExpr();
    

This code will give us an `SExpr` instance representing either the
s-expression that was parsed or a parser error. Let's try to evaluate the
s-expression to get a result back:

    
    
    var ev = new Evaluator();
    var result = ev.Eval(expr);
    Console.WriteLine(result);
    

This throws an exception at runtime, `"Undefined function 'prefix'"`,
indicating that we did not define the "prefix" function. Let's fix that by
defining our "prefix" function with the evaluator:

    
    
    var ev = new Evaluator()
    {
        { "prefix", (Evaluator v, InvocationExpr e) =>
        {
            if (e.Parameters.Length != 2) throw new ArgumentException("prefix requires 2 parameters");
    
            // Evaluate parameters:
            var prefix = v.EvalExpecting<string>(e.Parameters[0]);
            var list = v.EvalExpecting<object[]>(e.Parameters[1]);
    
            var sb = new StringBuilder();
            for (int i = 0; i < list.Length; ++i)
            {
                if (list[i].GetType() != typeof(string)) throw new ArgumentException("list item {0} must evaluate to a string".F(i + 1));
                sb.AppendFormat("[{0}].[{1}] AS [{0}_{1}]", prefix, (string)list[i]);
                if (i < list.Length - 1) sb.Append(", ");
            }
            return sb.ToString();
        } }
    };
    
    var result = ev.EvalExpecting<string>(expr);
    Console.WriteLine(result);
    

You can see how easily we're able to define MiniLISP-invokable functions from
C# code. The `Evaluator` class implements the `IEnumerable` interface and the
`Add` method required by C# to give us the dictionary initializer syntactic
sugar. Each object to add is a pair of the function name and the C# delegate
which is called when the evaluator invokes the function by that name. The
s-expression's function parameters are only evaluated by the delegate on
demand.

This "prefix" function we defined expects 2 parameters: the first a `string`,
and the second a list (typed as `object[]`). We evaluate both of those
parameter s-expressions using the `Evaluator` instance named `v` passed into
the function.

Then, for every item in the list, we make sure it is a `string` typed value,
then append it to our `StringBuilder` and format it with the prefix
appropriately, also inserting commas for separators.

Our resulting output for the example code above is:

    
    
    [st].[StudentID] AS [st_StudentID], [st].[FirstName] AS [st_FirstName], [st].[LastName] AS [st_LastName]
    

This should be perfect for inclusion in a SQL query.

    
    
    const string query = @"SELECT {prefix st [StudentID FirstName LastName]} FROM Student st WHERE st.StudentID = @studentID";
    

But parsing out that embedded MiniLISP code from the rest of the SQL syntax is
left as an exercise for next time.