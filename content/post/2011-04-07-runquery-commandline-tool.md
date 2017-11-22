---
author: jaymz
categories:
- Uncategorized
date: '2011-04-07T17:44:33'
tags:
- c# sql
title: RunQuery commandline tool
---
As a useful companion to <a title="LinqFilter binary release download link" href="http://bittwiddlers.org/viewsvn/trunk/public/LinqFilter/LinqFilter/bin/Release/LinqFilter-Release.zip?view=co&amp;root=WellDunne">LinqFilter</a> (which I have given considerable attention to over the last few weeks), I put together a tool called <a title="RunQuery binary release download link" href="http://bittwiddlers.org/viewsvn/trunk/public/RunQuery/RunQuery/bin/Release/RunQuery.zip?view=co&amp;root=WellDunne">RunQuery</a>, which, you guessed it, runs SQL queries from the console... BUT (and here's the kicker) it formats the results in an escaped, TAB-delimited format for direct use by LinqFilter.

For tool interoperability, I created (read: quite possibly reinvented) a useful, near-universal data interchange file format which uses TAB characters as column delimiters but also backslash escapes characters in each column to eliminate the possibility of actual TAB chars in the data interfering with the format. The backslash escaping rules are a subset of C#'s own string escaping rules. Also, null-valued columns are rendered as NUL (\0) chars in the format.

This data interchange format is accessible from LinqFilter via the <strong>SplitTabDelimited</strong> and <strong>JoinTabDelimited </strong>static methods. These methods are cloned in the RunQuery tool to ensure parity between the two tools. All of this makes it so we can easily exchange tabular data between tools using simple text pipes and/or files without having to worry about escaping, encoding, etc.

When you use RunQuery with default settings, it outputs two rows of header information. First, the column names, and second, the column types. All subsequent rows are column values. No row count is currently reported. There is no limit to number of rows returned either.

Aside from simply executing static SQL queries, RunQuery has a special "batch" mode which can execute a parameterized query whose parameter values are supplied via the stdin pipe. This mode expects the stdin lines to be fed in a particular order and formatting, namely the TAB-delimited encoding described above.

In this batch mode, the first line of input must be a row of parameter names where each column is a parameter name and must begin with a '@' character. The next line of input is a row of columns that represent parameter types. Acceptable values are the enum members of SqlDbType.

After these first two rows of metadata are consumed and validated, all subsequent rows are treated as columns of parameter values. For each row, the query is executed against the given parameter values until EOF is reached.

Example:

$ cat &gt; params
@n
int
1
2
3
^D

$ RunQuery -E -S myserver -d mydatabase <strong>-b</strong> -q 'select @n' &lt; params
---------------------------------------------------------------------------
select @n
---------------------------------------------------------------------------

int
1
2
3

For example, batch mode may be used for streaming query results from one server to another for something simple and naive like a poor man's data replication system or cross-database queries whose servers are not linked. One can also stream these parameter inputs from a LinqFilter query which may be processing input from a file or perhaps even consuming data from a web service. The possibilities are endless here.

Running RunQuery with no arguments displays some brief usage text.