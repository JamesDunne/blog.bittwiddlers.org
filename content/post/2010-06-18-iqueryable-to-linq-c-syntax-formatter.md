---
author: jaymz
categories:
- Uncategorized
date: '2010-06-18T15:25:37'
tags: []
title: IQueryable to LINQ C# syntax formatter
---
Technically, the title of this article should be
System.Linq.Expressions.Expression to LINQ C# syntax formatter but that's a
bit lengthy. To get right to the point, I've developed a class to parse an
Expression tree generated by a LINQ IQueryable query and produce C# LINQ
syntax as output. I tried searching the internet for similar work and nothing
was immediately available or that was in source form for me to integrate with
my tool. So, I wrote my own. Here's some sample output from the class:

    
    
    // C# LINQ query:
    var query =
         from x in (
              from ea in TableA
              join et in TableB on ea.TableBID equals et.TableBID
              join es in TableC on ea.TableCID equals es.TableCID
              join st in TableD on ea.TableDID equals (int?)st.TableDID
              join sf in TableE on ea.TableEID equals (int?)sf.TableEID
              join eb in TableF on ea.TableFID equals eb.TableFID
              join psf in TableF on ea.PreviousTableFID equals (int?)psf.TableFID into temp from ps in temp.DefaultIfEmpty()
              select new { ea = ea, et = et, es = es, st = st, sf = sf, eb = eb, ps = ps }
         )
         where ((x.ea.DueDate < (DateTime?)DateTime.Now.Date) && (x.es.Code != "C"))
         select x
         .OrderBy(a => a.ea.DueDate)
         .Skip(20)
         .Take(10);
    

This class is intended primarily for display purposes. It should not be used
in its current state for attempting to write out compilable C# LINQ syntax.
Download the code:

  * [2010-06-18-LinqSyntaxExpressionFormatter](http://bittwiddlers.org/wp-content/uploads/2010/06/2010-06-18-LinqSyntaxExpressionFormatter.zip)