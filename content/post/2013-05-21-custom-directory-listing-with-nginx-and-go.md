---
author: jaymz
categories:
- Uncategorized
date: '2013-05-21T13:49:52'
tags: []
title: Custom Directory Listing with Nginx and Go
---
For the last few years, I've been maintaining a [large repository of files and
folders](http://bittwiddlers.org/ftp/) on my website here using `lighttpd`'s
default directory index generator. The generator is fine to get the job done,
but offers no extra features. I just recently switched to `nginx` and its
directory index generator is a bit worse than `lighttpd`'s (the `autoindex`
directive). This approach worked fine for a while but I really wanted the
option to have a custom file ordering for certain directories, e.g. to order
by date descending so newer files would automatically float to the top of the
file list. So I [wrote a HTTP server](https://github.com/JamesDunne/go-index-
html) in [Go](http://golang.org/) to do just that, and a little more!

This project was my first real foray into the [Go programming
language](http://golang.org/) (which I have a few choice opinions about but
I'll express those in another post later). For the most part, the experience
has been pleasant, save for a few language warts. The Go runtime is rock solid
and my HTTP server has not gone down at all. I keep it running with `upstart`
on my Ubuntu server. If you're not managing your daemons with `upstart`, you
definitely should start. It's far easier than the horrible copy/paste/modify
workflow of those awful init.d scripts.

What I do is have `nginx` act as a reverse proxy for `/ftp/` requests to my Go
HTTP server which is just listening on a localhost port. I intend to change
this over to use local Unix sockets for more security and to save my sanity in
dealing with TCP port numbers and remembering which one goes where.

The main features of this directory listing generator are custom ordering of
files per directory and slightly advanced symlink support.

To specify a custom ordering for a directory, just create a file named
`.index-sort` in the directory and have its contents be a single line
specifying the sort mode. The available sort modes are documented on the
GitHub project's [README](https://github.com/JamesDunne/go-index-
html/blob/master/README.md). To override the default sort order, you can
specify the `?sort=mode` query string parameter in the request.

The advanced symlink support helps to translate filesystem symlinks into HTTP
302 redirects. This works for both files and directories. If the symlink
target path is within the filesystem jail being served up, the request will be
served, otherwise a `400 Bad Request` error will be presented.

For example, if you have a set of versions of some file and a symlink that
always points to the latest version, the directory listing will 302 redirect
from the symlink request to the actual target filename that is the specific
version. In other words, a request to `file-latest.kind` might redirect to
`file-v1.kind`. This way, the downloaded filename will represent the symlink
target `file-v1.kind` and you can be sure which specific file your users have
downloaded, instead of the file being served up as `file-latest.kind` and you
having no clue which one that represented at the time the user downloaded the
file.

I'm really pleased with this setup and it took me only a few hours to code up
and test. Go does allow one to be productive right off the bat. Best of all,
there's no funny business about threading, concurrency, or reliability like
you get with other things like Ruby or Python (mostly the concurrency issue
here). There's just fast, compiled, statically typed code here; just the way I
like it. Of course Go isn't perfect, but we'll get into that later.

Feel free to use this process for hosting your own directory listings. I look
forward to the pull requests!