---
author: jaymz
categories:
- Uncategorized
date: '2013-05-20T16:13:04'
tags: []
title: Goodbye lighttpd; hello nginx
---
![enter image description
here](http://3.bp.blogspot.com/-vE0QC2PAc3s/T_8QM8gIwyI/AAAAAAAABEk/fVDnp0CLva8/s1600/change.gif)

.

.

.

![enter image description
here](http://2.bp.blogspot.com/-Iw67tDAvy2Y/UJvr44XrxoI/AAAAAAAABKA/JJPt_qvyoqI/s320/lisp.gif)

It took me a while (collectively ~8 hours), but I've finally replaced
`lighttpd` with `nginx` on this server!

`nginx` is already using vastly fewer resources than `lighttpd` ever did on
its best day. I'm happy about that considering the limited resources this
server has (`MemTotal: 1008568 kB`). I'm also pleased with the way `nginx`
handles basic things in a zero downtime manner, e.g. reloading configuration
files. I hated that I always had to completely kill `lighttpd` and restart it
just to reload the configuration file for a minor change. `nginx` reloads the
configuration file transactionally and will rollback if issues are found. That
alone is worth switching for if you're on the fence.

Getting `nginx` to match my existing `lighttpd` configuration was a bit of a
challenge but I got it all sorted out in the end. Some issues I faced were in
getting PHP requests through to `php-fpm`. Those issues were mostly due to
`nginx`'s quirky `root` and `alias` directive behavior, especially regarding
the request handling cycle and nested location tags and all the internal
redirections and regexes required. (I **HATE** regexes.)

I settled on a very simple albeit repetitive configuration. There's no global
`root` directive. All the main `location` directives are independent of one
another, which works best for my setup since I have WordPress as the root `/`
with other sites "grafted" on from there. The PHP-specific `location`
directives are copy/pasted and nested into each main `location` directive as
needed.

The trickiest part was getting PHP requests with PATH_INFO (e.g.
`/index.php/2013/05/article-name`) to work. I found the default example in the
`nginx` documentation for `fastcgi_split_path_info` and it works great.

For those who are curious and just want to see the `nginx.conf` details, here
you are!

    
    
    server {
        listen       64.85.164.128:80;
        server_name  bittwiddlers.org;
    
        location / {
            root   /var/www-bittwiddlers/wordpress;
            index  index.php;
    
            location ~ ^.+\.php {
                try_files $uri /index.php;
    
                fastcgi_split_path_info ^(.+\.php)(/?.+)$;
                fastcgi_pass   unix:/tmp/php5-fpm.sock;
                fastcgi_index  index.php;
                include        fastcgi_params;
                fastcgi_param  SCRIPT_FILENAME $document_root$fastcgi_script_name;
            }
        }
    }
    

There are a few other main `location` directives, but they're irrelevant to
the WordPress setup so I've omitted them here.

My `fastcgi_params` file is almost exactly the default file that comes with
nginx, except the SCRIPT_FILENAME line is commented out. I've found that the
best way is to specify this param per each `location` directive.
`$document_root` does not work when you only have an `alias` directive and no
`root` directive. It will only have a value if a `root` directive exists.

For my configuration I've abandoned `alias`es entirely because of the PHP
configuration issues they caused. This is most unfortunate because it should
just be a simple thing to set up, but it is not.

Another minor issue that bit me was configuring HTTP Basic Authentication.
`lighttpd` and `nginx` handle this differently regarding the passwd files that
store the username/passwords. `nginx` is a little more obsecure* (conjunction
of obscure and secure, implying security via obscurity) than `lighttpd` in
that it requires that passwords in the `htpasswd` file are "encrypted" so you
have to use the `htpasswd` tool to create those entries. `lighttpd` is a
little more lax in that it doesn't care at all.

What also irked me is that `nginx` has no equivalent to `lighttpd`'s
`"require" => "user=username"` feature. I was using that feature in `lighttpd`
to "secure" some parts of the site down to specific users while using one
common `htpasswd` file. For `nginx` I had to separate the `htpasswd` file into
multiple files, one for each section. This was a little annoying but not
really a big deal.

What am I doing "securing" things with HTTP Basic Auth, you ask? I'm taking
the most primitive security measures to protect access to those things which
deserve only such primitive security measures. In other words, the measure is
consistent with the value I place on the secured data. :)