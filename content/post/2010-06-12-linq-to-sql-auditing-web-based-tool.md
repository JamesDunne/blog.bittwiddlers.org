---
author: jaymz
categories:
- Uncategorized
date: '2010-06-12T00:32:34'
tags: []
title: LINQ-to-SQL auditing web-based tool
---
I've been developing a LINQ-to-SQL auditing web-based tool for the last few
days and I'd like to share some progress with you all. This auditing tool
takes an instance of our data repository implementation class, finds all
public methods via reflection, and executes them one-by-one with a special
auditing mode enabled. Each of our query methods is required to call one of
our various audit methods depending on the query execution scenario. For
instance, if enumeration over an IQueryable is needed to pull back multiple
results, call AuditEnumerable(query) on that query. If a stored procedure
needs to be executed, call AuditStoredProcedure(db =>
db.MethodToCallProcedure(prm1, prm2)). If you need a single result, either
null or an instance, call AuditSingle(query). Needless to say, all of our data
repository methods have been made to follow development patterns such that no
query execution is performed while in auditing mode. Conversely, if the
auditing mode is turned off, the methods should behave as normal and return
actual data from a connection to SQL Server. This web page is deployed as a
single aspx file with no code-behind for "mobility" purposes. It is currently
deployed, for better or worse, in our internal WCF ASP.NET services host
project right next to our *.svc files that host the service responsible for
connecting to our data repository methods. The nice thing is that this tool is
zero-configuration and Just Works (TM). It is contained all in one file and
can be deployed virtually anywhere in any environment. The main interface for
the tool is a paged listing of our data repository methods and the auditing
output per each method. An example:

### IPagedData<ActivityDetails> GetT0sByT1IDTypeID(T1ID id0, T2ID id1, T3ID
id3, PagingInfo paging, T0SortingInfo sorting)

    
    
    -- This is a PAGING implementation.
    DECLARE @p0 int;
    DECLARE @p1 int;
    DECLARE @p2 int;
    DECLARE @p3 int;
    
    
    SET @p0 = 777;
    SET @p1 = 778;
    SET @p2 = 20;
    SET @p3 = 10;
    
    
    SELECT [t8].[ID], ...
    FROM (
        SELECT ROW_NUMBER() OVER (ORDER BY [t0].[OrderByColumn]) AS [ROW_NUMBER], [t0].[ID], ...
        FROM [dbo].[Table0] AS [t0]
        INNER JOIN [dbo].[Table1] AS [t1] ON [t0].[T1ID] = [t1].[ID]
        INNER JOIN [dbo].[Table2] AS [t2] ON [t0].[T2ID] = [t2].[ID]
        INNER JOIN [dbo].[Table3] AS [t3] ON [t0].[T3ID] = [t3].[ID]
        INNER JOIN [dbo].[Table4] AS [t4] ON [t0].[T4ID] = [t4].[ID]
        INNER JOIN [dbo].[Table5] AS [t5] ON [t0].[T5ID] = [t5].[ID]
        LEFT OUTER JOIN (
            SELECT 1 AS [test], [t6].*
            FROM [dbo].[Table6] AS [t6]
            ) AS [t7] ON [t0].[T7ID] = ([t7].[ID])
        WHERE ([t0].[T0ID] = @p0) AND ([t0].[T1ID] = @p1)
        ) AS [t8]
    WHERE [t8].[ROW_NUMBER] BETWEEN @p2 + 1 AND @p2 + @p3
    ORDER BY [t8].[ROW_NUMBER];
    GO

This is only a part of the output of the tool so far. I've also trimmed some
of the more sensitive information from the query. I blogged about the SQL
syntax highlighter I developed in my previous post so if you're curious how I
did that go [back one post](http://bittwiddlers.org/?p=110) from here. The
method names in the listing page are enabled as hyperlinks to go into a
"details" mode. In this details page, you can actually fill in parameter
values (and properties found on classes for those parameter types) used to
alter the generated SQL query. You can even Execute the method with your
parameters and see the resulting output formatted as XML. We've enabled all of
our domain models that our data repository methods work with to be
[DataContract] decorated. This gets us the ability to serialize these objects
into whatever format we desire at any time. I've chosen to display the results
as XML for simplicity's sake. I could even format the execution results as
JSON using the DataContractJsonSerializer and pass that data into a jQuery-
driven data table implementation for a more familiar grid-like view, not to
mention a much more user-friendly interface to work with. I've got numerous
plans for features I'd like to implement for this tool. I will consider
releasing some of the framework and tool code if I can lighten up some of the
dependencies involved.