---
author: jaymz
categories:
- Uncategorized
date: '2009-03-04T02:20:08'
tags: []
title: libevent for .NET / C# / Managed C++
---
Ever wanted to write your own scalable .NET managed server applications?  You can  using libevent's .NET managed wrapper that I just wrote!

libevent is a C library useful for creating extremely scalable server applications in unmanaged code.  I couldn't find a managed .NET wrapper for this library so me being Mr. NIH I invented it myself.   In actuality, it wasn't that bad!  Managed C++ is an interesting beast.

For the TL;DR crowd, here's the <a href="tsvn:svn://bittwiddlers.org/libeventsharp/trunk/">SVN repositor</a>y for the project including source code and compiled assembly.  This repository is hosted on my own VPS.

You'll want to start by looking at the LibEventSharp.Test project included in the distribution.  This is a simple accept-connections and echo back the input example.

libevent contains many more functions than I included in this wrapper at this time, but I did get most of the core functionality in there.  With this, one can write a scalable managed server application without bothering with any one of the messy details of the common MS offerings in this area (e.g. ASP.NET, TcpServer, Remoting, WCF)!