---
author: jaymz
categories:
- Uncategorized
date: '2011-03-09T10:42:17'
tags: []
title: LinqFilter Code Injection
---
Unless you've been living under a rock, you may have heard of such things as SQL Injection and XSS (cross-site scripting) attacks, just to name the popular forms of the attack. These types of attacks are language specific forms of the general "Code Injection" attack. These types of attacks happen when developers concatenate source code with unchecked user input and get to the stage where that fully trusted source code is eventually executed.

SQL Injection happens when a developer concatenates user input into a SQL Query but the user input may contain special characters like single-quotes which changes the meaning of the query. You could do this in a safe way by either properly escaping the raw user input or passing it in to the SQL Server out-of-band in a parameterized query. But I'm not here to blather on about SQL Injection attacks.

Today I'd like to talk about <strong>C# Code Injection</strong>, specifically as it applies to <a title="Blog Link" href="http://bittwiddlers.org/?p=141">LinqFilter</a>.

In LinqFilter, I naively concatenate what should be untrusted user input LINQ code directly into an implicity trusted C# code template embedded in the program. I invoke the C# compiler provider on that resulting string and execute the code in the compiled assembly. By concatenating unchecked user input into an existing code template, I've opened the door to C# Code Injection. The user has numerous ways of defeating the requirement that the query be a valid C# 3.5 expression.

One can easily terminate the expression with a ';' and go on to write regular C# statements. LinqFilter will naively concatenate your code that it assumes is an expression into the code template and compile it. The compiler doesn't care what happened so long as the code compiles as a valid C# program. This defeats the purpose (somewhat) of the LinqFilter tool so it's generally not wise to do so and you'd only be hurting yourself, unless of course you're executing someone else's malicious LinqFilter scripts.

What I'd really like to do is put in a safety check to validate that the user input is indeed a C# 3.5 expression. This involves getting a parser for the C# 3.5 language, not an easy task. All I need to do is simply validate that the input parses correctly as an expression and only an expression. I don't need to do any type checking or semantic analysis work; the compiler will handle that. I just need to know that the user can't "escape out" of the expression boundaries.

Unfortunately, there are no prepackaged C# parsers that I could find. This means using one of the plethora of available parser-generator toolkits or writing my own. Neither options are sounding very attractive to me at the moment so I'll probably just hold off entirely on the work since it provides no benefit to myself. I know how to use LinqFilter effectively and there's clearly no community around it (yet) that are sharing scripts so there's really no security problem. If someone else is concerned enough about the problem, I gladly welcome patches :).

I think I'd also like to figure out how the CAS (code access security) feature works and incorporate that into the generated code so that queries are executed in a safe context and won't be going out and destroying your files by invoking File.Delete() or something sinister, but this is a low priority as well.