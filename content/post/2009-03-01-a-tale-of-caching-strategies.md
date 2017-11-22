---
author: jaymz
categories:
- Uncategorized
date: '2009-03-01T19:34:57'
tags: []
title: A tale of caching strategies
---
Let me regale you with the tale of my most recent endeavour into the realm of caching strategies...

In developing an in-house content management system for ASP.NET, I knew that there was going to be lots of content developed and stored in the database-backed system.  I also knew that the system would need to be deployed on a web farm, so the default in-memory ASP.NET cache provider was not going to serve me well here.  A distributed caching approach was necessary here, so I found memcached after some vague googling.

From 20,000 ft. up things look good now.  We have the cache consistency problem solved without too much overhead.  However, what is our data caching strategy?  That is, what data do we want to store into the cache and how do we want to store it and later retrieve it consistently?  Just storing data into the cache is useless unless you have a way to get it back out, right? :)

My first whack at a caching strategy was to cache database table rows one by one based on table name and identity key value of the particular row.  Each query would only return back identity values instead of fully populated result sets.  For each identity value I would ask the cache for each object.  If the cache would miss, then I'd go ask the database for the row based on the identity value and then insert that data into the cache.

Let me tell you know, this is a terrible caching strategy and I will never employ it again.  In fact, I recently just ripped out this stupidity and went with a much better strategy.

The thought was that I'd be reducing the number of keys which actually contained row data to one.  This would make for an easy way to invalidate the cached data once it was updated by one of the web server nodes.  This was about ALL that was easy to do with this caching strategy.  Caching and invalidating lists of identity values from queries was a nightmare.  Each time you created/delete a table row, you'd have to explicitly invalidate all of the cache keys which might be affected query-wise from this row addition/deletion, if you could!

A superior strategy (at least in this case) is to employ <strong>generational caching</strong>.  I'll tell you now, you won't find much information on this strategy and I honestly cannot tell you why.  Probably because it is so simple an idea as to be deserving of many names with which to identify it.

The idea is this:

First, think about how your data to be cached is partitioned into logical subdomains.  For instance, my CMS system has scheduling information, virtual page layout information, XHTML content and metadata, and templates.

For each subdomain, keep a simple counter value stored in a globally accessible location, i.e. a database.  Every time you make a change to data within that subdomain, increment that counter's value and store it back to the database.  We'll call this value a generational counter.

Whenever you need to store data related to that particular subdomain in your cache, grab the most recent value of that generational counter from your database and include its value in your cache keys for all data related to that subdomain.

Here's the kicker... Whenever you request data from the cache, always build the cache key using the current value of the generational counter, which may now be different from the old key name(s) stored in cache!  This makes it so all your data is effectively instantaneously invalidated simultaneously.  You'll likely never have to worry about retrieving stale data from the cache.  You'll also never have to worry about tracking the invalidation of cached data yourself!

Of course, this strategy only works well when read performance is more critical than write performance for your particular needs.  CMSes generally do more reading than writing.

Using either more subdomains or using groups within subdomains of the data to be cached will help keep your cache hit to miss ratio higher at the expense of managing the potential complexity of defining these groups.