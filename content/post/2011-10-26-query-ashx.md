---
author: jaymz
categories:
- Uncategorized
date: '2011-10-26T17:55:33'
tags: []
title: query.ashx
---
I put together a [web-based SQL query tool](https://gist.github.com/1286172
"gist for the code") written using ASP.NET’s IHttpHandler interface, packaged
as a single query.ashx file. Its main feature is that it guarantees that you
cannot construct a query that inserts or updates data. The SELECT query form
is forced upon you and any means of escaping that form via SQL injection is
detected and the query is rejected in that case. This makes it safe to deploy
internally so that your developers may query data in a read-only manner.

Unfortunately I can’t link you to any public deployment of this tool for a
demo because it queries SQL databases. I don’t have any SQL databases out in
the wild and if I did I certainly wouldn’t want any random jackhole off the
internet querying data from them anyway. You’ll just have to deploy it
yourself for your own demo. It’s not hard to set it up since all you need is
just some form of ASP.NET host that can compile and execute an ashx file.
Virtually any standard IIS host will be able to handle this in its default
configuration. The tool runs under the security context of the app pool you
place it in so if you use connection strings with Integrated Security=SSPI,
beware of that.

# Features

  * SQL query builder that allows only SELECT queries (Query tab) 
    * Try to break it and find a way to UPDATE or INSERT data! 
    * Strips out all SQL comments 
    * Actively prevents you from overriding the forced separation of query clauses, e.g. you cannot put a FROM into the SELECT clause unless it’s part of a subquery 
  * Results tab 
    * Queries are forcibly executed in READ UNCOMMITTED transaction isolation level 
    * SET ROWCOUNT is set to 1000 by default but can be overridden with rowlimit=# query string parameter. 
    * Dynamic show/hide of SQL column type headers 
    * Execution time tracked in msec 
    * Results grid with left/right text alignment set per column type (numerals are right-aligned, text is left-aligned, etc.) 
    * Binary data is displayed as 0xHEXADECIMALSTRINGS 
    * Shows generated SQL query 
    * Link to share query with someone else (opening link automatically executes the query) 
    * Links to JSON and XML output formats with appropriate Content-Type headers. (Try it in Chrome’s Advanced REST Client app!) 
  * Custom Connection String support (Connection tab) 
    * The drop-down is sourced from web.config’s connectionStrings section 
  * Recording of query history (Query Log tab) 
    * Stores host name of client that executed query 
    * Paged view 
    * Click “GO” link to execute query 
  * Parameterized queries 
    * Parameter values are saved in the query log 
    * Probably want to store a library of queries in addition to the query history log 
    * Limited set of parameter types supported, but most common ones used should be there 
  * Multiple forms of JSON and XML formatted output for programmatic consumption of query results 
    * Add query string params to produce less verbose output: no_query=1, no_header=1 
  * Single-file deployment 
  * Self-update feature that pulls latest code from [github](https://gist.github.com/1286172)
  * Tip jar! 

I rather enjoy writing developer tools like this. I especially enjoy the ashx
packaging mechanism. For an ashx file, you write all your tool code in one
file that contains a primary class that implements the IHttpHandler interface
(or IHttpAsyncHandler). You have complete control over the HTTP response in
this way at a very low level yet you still get all the convenience of the
HttpContext class with its Request and Response structures that ease the pain
of dealing with HTTP headers, URLs, query strings, POST form variables, etc.
at such a low level.