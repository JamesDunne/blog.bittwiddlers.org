---
author: jaymz
categories:
- Uncategorized
date: '2010-03-30T18:09:15'
tags:
- code generation
- fxcop
- reflection
- t4
title: T4 templates with reflection
---
After redesigning our data, business, and services layers to be interface driven, I wanted to keep the amount of boilerplate code written by developers down to an absolute minimum. The best way I know how to do this is through code generation.

T4 templates are a good solution to this problem. T4 might not be as well known as other popular code generation tools but the fact that you get it for free and it's integrated into Visual Studio is a very strong selling point in my opinion. The less impact it has on your existing development environment requirements, the better.

When you start thinking about generating code based on compiled code in an assembly, the term "reflection" should spring to mind. If it doesn't, this article isn't for you.

If you have had previous experience with .NET's built-in reflection API you might be aware of the small snag whereby assemblies loaded for reflection are locked for exclusive access and cannot be unloaded until the AppDomain which loaded them is torn down.

For T4 templates this poses a somewhat significant problem. What problem is this? The T4 template generator invoked by Visual Studio is run in its own AppDomain and that AppDomain instance is kept around and recycled once every 25 uses. If you were to use reflection in your T4 template and load up your assembly, you would not be able to recompile that assembly until the 25th run of the T4 template tool. Yuck.

You might be saying at this point, "well now wait a minute... you can create a temporary AppDomain for reflection, do your reflection work in there, and tear down the temporary AppDomain to solve that problem!" ... and you may be right. As for me, I've attempted that solution myself to no avail and was frought with nasty restrictions here and there. In theory it should be quite simple. In practice it is quite the daunting nightmare. Trust me, you don't want to go there. Pain and suffering awaits you on that dark path. Be warned.

So, now that reflection is out, what's left? After much googling of the issue I discovered the Microsoft.Cci.dll introspection API used by FxCop.

This assembly exposes a managed code library that is able to read .NET type metadata from an assembly file without actually loading the assembly into an AppDomain prepared for execution. It does this by opening the file as a binary stream, parsing out the metadata information in the stream and keeping it in memory, and closing the file. No nasty file locks are held on the assembly file. This suits our requirements for the T4 templating system quite nicely and is in fact very speedy, or rather speedy enough for code generation tasks.

A significant down-side to using this 3rd party API is that it is not reflection, and it has its own set of types for representing metadata. It does, however, look similar in many ways to reflection. There is no documentation that I'm aware of, but using a free copy of .NET Reflector from RedGate on the Microsoft.Cci.dll assembly itself should get you enough information on how to use the API.

Another pain point in using T4 templates is that there is no intellisense support in Visual Studio for editing them, so it's back to old-school days of writing code in a dumb text editor, praying it will compile, and execute properly. There's also not a great story around debugging code in the T4 templates either. I've managed to hobble along in debugging my own templates, but it would be a maintenance nightmare if you were to have multiple developers working on these templates.

Next post I might share with you some of my generic Microsoft.Cci.dll common utility methods and some example code.

To find Microsoft.Cci.dll you'll need a copy of the latest FxCop tool. Ill have to check the version number I downloaded but I do remember that an older version of the dll included some nastiness that breaks C# 3.0 usage of the 'var' keyword. That compiler error will surely throw you for a loop if you're not prepared. Not to fear, however, the latest version fixes this problem.