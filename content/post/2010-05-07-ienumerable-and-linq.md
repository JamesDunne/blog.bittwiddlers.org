---
author: jaymz
categories:
- Uncategorized
date: '2010-05-07T08:18:15'
tags:
- c#
- ienumerable
- linq
title: IEnumerable and LINQ
---
I cannot stress enough the importance of knowing how LINQ queries work when they are based on an IEnumerable source.

When one defines a query based on an IEnumerable source, the query variable represents just that: the query, NOT the results of enumerating the query.

Each time you enumerate over the query object, you are calculating the results of that query on-demand. There is no caching of results. The LINQ IEnumerable implementation makes no assumptions that enumerating the same query twice in a row will produce the same results and so it let's you do so without any qualms.

If what you meant to do was to run the query once and store the results for future operations to work on, then creating a List<T> variable to store those results in memory sounds like a reasonable approach to solving this problem. List<T> implements IEnumerable<T> so it is a good candidate for replacement in future LINQ queries that want to work on the results of the first query, not having to constantly recompute that query itself multiple times.

var query = from x in something select x;
var results = query.ToList();

Use the `results` variable when you want to reference the results of `query`.

The same restrictions apply to IQueryable LINQ queries. When enumerating over an IQueryable, it calls the underlying IQueryProvider to transform your query operations into whatever form is best for that provider to execute your query against its data source.

Be *very* careful when including IQueryable variables in another IQueryable LINQ query because you will be effectively telling your query provider to combine those queries together, and it will be up to the query provider to figure out how to do so or to raise an exception telling you that that's unsupported or simply not possible. If you rather meant to pass the results of that query into another then you should use the AsEnumerable extension method. That should guarantee that the two queries are kept independent and that the results of the first are fed into the second.

As an aside, the ToList extension method *always* creates a new List<T> instance, regardless of the type of IEnumerable<T> it is called on. Be careful when using this method too many times because you will create a new list each time it is called. This can be very wasteful with memory.

Also, when taking an IEnumerable<T> as a parameter to your method, it would be wise to provide the guarantee to the caller that it will be enumerated only zero or one times. You can accomplish this by being careful in your implementation or by using the ToList extension method to work on a local List variable when you know you need to work on the set more than once.