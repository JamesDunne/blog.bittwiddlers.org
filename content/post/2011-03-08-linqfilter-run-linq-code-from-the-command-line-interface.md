---
author: jaymz
categories:
- Uncategorized
date: '2011-03-08T15:04:16'
tags:
- bash
- c#
- CLI
- linq
title: 'LinqFilter: Run LINQ Code From The Command Line Interface!'
---
Having recently acquired a taste for using git on Windows with msysGit, I've
been getting a lot more productive with my use of bash and other command-line
tools in Windows. Shifting data around on the command line gets pretty hairy
very quickly. Unfortunately, the basic set of Un*x utilities that process text
data is just not powerful/flexible enough and usually each tool has some
ridiculous custom syntax to learn, all of them different. I already know a
language powerful enough to process text efficiently, succinctly, and cleanly:
LINQ! So I thought to myself, why not take advantage of LINQ to write simple
little one-off text-processing scripts? Creating a new console application
every time to handle this task becomes arduous, to say the least. Enter:
**LinqFilter**! For the impatient, you may download the latest release of the
tool
[here](http://bittwiddlers.org/viewsvn/trunk/public/LinqFilter/LinqFilter/bin/Release
/LinqFilter-Release.zip?view=co&root=WellDunne "Download ZIP") (ZIP download).
If this tool ever becomes popular enough, I have no problems hosting it
elsewhere. LinqFilter is, in a nutshell, a way to dynamically compile user-
supplied C# v3.5 LINQ code and execute it instantly, sending the resulting
item strings to Console.Out delimited by newlines or custom delimiters. An
input IEnumerable<string> named lines is provided to allow the query to read
lines from Console.In. There are many command-line options available to
customize how LinqFilter behaves. Let's take the following example LINQ query:

    
    
    LinqFilter -q "from line in lines select line"

This is a simple echo query. It will echo all lines read in from Console.In to
Console.Out and it will do so in a streaming fashion. There is no storage of
lines read in or written out. As a line comes in, it is run through the query
and written out. The "-q" command-line option appends a line of code to the
((QUERY)) buffer. You could supply multiple lines of code by supplying
multiple "-q" options in order. How does this work? The LinqFilter tool
basically concatenates your query code into the following abbreviated class
template:

    
    
    public static class DynamicQuery {
        public static IEnumerable<string> GetQuery(IEnumerable<string> lines, string[] args) {
            ((PRE))
            IEnumerable<string> query = ((QUERY));
            ((POST))
            return query;
        }
    }

The ((QUERY)) token is your query expression code. The ((PRE)) token is
replaced with lines of C# code you supply in order to do one-time, pre-query
setup and validation work. The ((POST)) token is replaced with lines of C#
code you supply that takes effect after the query variable is assigned. This
section is rarely used but is there for completeness. As you can see, the
query is enclosed in a simple static method that returns an
IEnumerable<string>. The host console application supplies the lines from
Console.In, but your query is not required to use that and can source data
from somewhere else, or make up its own. :) The args parameter is used to
collect command-line arguments from the "-a <argument>" command-line option so
that the query may be stored in a static file yet still use dynamic data
passed in from the command line. Let's look at an example with a ((PRE))
section:

    
    
    LinqFilter -pre "if (args.Length == 0) throw new Exception(\"Need an argument!\");" -q "from line in lines where line.StartsWith(args[0]) select line.Substring(args[0].Length)" -a "Hello"

Here we put in a full C# statement in the ((PRE)) section via the "-pre"
command-line option to handle validation of arguments. The query itself is a
simple filter to only return lines that start with args[0], i.e. "Hello". The
best feature of the tool is the ability to store your queries off into a
separate file and use the "-i" parameter to import them. Let's leave that for
another time. In the mean time, I encourage you to download the tool and
explore its immense usefulness. I must have written 30 or so one-off queries
by now. I find new uses for it every day, which makes it a fantastic tool in
my opinion and I'm very glad I took the time to write it. I hope you enjoy it
and find it just as useful as I have! P.S. - if you ever get lost, just type
LinqFilter --help on the command line with no arguments and a detailed usage
text will appear. :)