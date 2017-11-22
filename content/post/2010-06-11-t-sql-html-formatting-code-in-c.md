---
author: jaymz
categories:
- Uncategorized
date: '2010-06-11T23:20:42'
tags: []
title: T-SQL HTML formatting code in C#
---
As usual, when I blog, I only blog about things I find that are unique and
that haven't been posted before. Today is no exception. I give you an HTML
formatter for T-SQL, written in C# using Regular Expressions.

    
    
    public static string HTMLColorizeSQL(string sql)
    {
        string output = HttpUtility.HtmlEncode(sql);
        output = Regex.Replace(output,
            @"^--(?<comment>[^\r\n]*)(?<post>\r\n|$)",
            @"<span class=""sql_comment"">--${comment}</span>${post}",
            RegexOptions.IgnoreCase | RegexOptions.Multiline
        );
        output = Regex.Replace(output,
            @"(?<=(\[|\b))(?<keyword>(SELECT|FROM|WHERE|ORDER|INNER|JOIN|OUTER|LEFT|RIGHT|CROSS" +
                @"|DISTINCT|DECLARE|SET|EXEC|NOT|IN|IS|NULL|BETWEEN|GROUP|BY|ASC|DESC|OVER|AS|ON" +
                @"|AND|OR|TOP|GO|CASE|WHEN|ELSE|THEN|IF|BEGIN|END|LIKE))\b",
            @"<span class=""sql_keyword"">${keyword}</span>",
            RegexOptions.IgnoreCase
        );
        output = Regex.Replace(output,
            @"(\b(?<keyword>ROW_NUMBER|COUNT|CONVERT|COALESCE|CAST)(?<post>\())",
            @"<span class=""sql_function"">${keyword}</span>${post}",
            RegexOptions.IgnoreCase
        );
        output = Regex.Replace(output,
            @"(?<param>\@[\w\d_]+)",
            @"<span class=""sql_param"">${param}</span>",
            RegexOptions.IgnoreCase
        );
        return output;
    }

Please allow me to express how much I HATE the use of regular expressions for
parsing tasks like this. Normally I would be completely content to sit down
and hack up a custom parser in C# for T-SQL or at least try to generate one
from existing tools. I figured the most I'm trying to accomplish here is
syntax highlighting, so what's the harm in going with Regex here? Note that
this implementation comes with a few caveats.

  1. This is not a full keyword or function list, but seems to be enough to cover what LINQ-to-SQL would generate.
  2. The <span> tags are applied even within 'sql_comment' spans, so some clever CSS selector trickery is required to properly format comment lines.
  3. Things that are NOT highlighted because LINQ-to-SQL does not generate them in its output: /* comments */ are not highlighted. SQL server type names are not highlighted. SQL strings are not highlighted.

The CSS that I am using is in this form:

    
    
    /* SQL keyword: a span with a 'sql_keyword' class not found within an that has a 'sql_comment' class */
    :not(.sql_comment)>span.sql_keyword
    {
    	color: #33f;
    	font-weight: bold;
    }
    
    /* SQL function: a span with a 'sql_function' class not found within an that has a 'sql_comment' class */
    :not(.sql_comment)>span.sql_function
    {
    	color: #3f6;
    	font-weight: bold;
    }
    
    /* SQL parameter */
    :not(.sql_comment)>span.sql_param
    {
    	color: #993;
    	font-weight: bold;
    }
    
    /* SQL single-line comment */
    .sql_comment
    {
    	color: Olive;
    	font-weight: bold;
    }

The key pattern to take away here is the use of the :not(x) CSS selector. It
says to format a keyword, parameter, or function name if and only if the span
is not contained within a span with the class 'sql_comment' applied. I hope
you enjoy that and I hope to see future improvements coming back this way!