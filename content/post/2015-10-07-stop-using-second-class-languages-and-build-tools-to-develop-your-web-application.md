---
author: admin
categories:
- Uncategorized
date: '2015-10-07T16:40:49'
tags: []
title: Stop using second-class languages and build tools to develop your web application
---
Please, stop using all this HTML middleware to develop your web application.
CoffeeScript, SASS, LESS, HAML, Middleman, Bower, Grunt, etc. Stop it.

No, it doesn't make things easier for the developer. Sure, it might make
things easier for YOU, A developer, but not for all developers or future
maintainers. I don't want to learn yet another silly combination of second-
class languages to figure out what your application is doing. Just write
vanilla JavaScript. Just write vanilla CSS. Just write vanilla HTML.

Stop with the middleware nonsense. It's not necessary and it actively harms
the maintainability of the application, not to mention that it certainly puts
a shelf life on the application as a whole. How long will these second-class
languages be around for? Will they be supported in the future? Are they going
to completely change their inner/outer workings and break your application in
two years?

There are several very valid and very strong reasons against using all these
middleware and second-class languages.

## Development Environment

One big strike against using middleware and second-class languages is that the
development environment becomes virtually impossible to reproducibly install
and run across all platforms.

Installing just one tool like coffeescript means that you need to specify
exactly which version of coffeescript worked for you on the platform you
developed on plus the specific version of node.js required to execute that
coffeescript version. The standard package managers for other platforms might
not carry that version yet, leading you into a yak-shaving exercise to go find
the proper version for your platform, maybe compile it, install it, manage it
in a virtual environment of some sort so it doesn't conflict with a pre-
existing installation of a specific version required for a different project
you might be working on, etc.

As for coffeescript itself, I feel it's on its way out in light of recent
advancements in JavaScript starting with ECMAScript 6. We don't need some
silly syntax translator. We can write JavaScript just fine now. Sure it's more
typing for you but code is written to be read far more often than to just be
written.

Maybe you decide to go with some middleware management tool like Middleman.
That will require a ruby installation of a very specific version so that all
the features you rely on will work, and of course requires that you know a bit
of ruby to work with the config.rb configuration file. Well, I've never
touched anything ruby before, and I really don't care for it so I've never
bothered to learn it. Granted it's not that hard to pick up on and follow some
patterns to modify some stuff in the config.rb, but if I have to dig deep and
really change the guts of how the application wants to be configured, I'm at a
loss for time and effort to go do that.

Let's say I have no time to dedicate towards learning CoffeeScript, or HAML,
or SCSS, and I just want to statically recompile all these second-class
languages into their first-class counterparts and ditch the second-class
language code entirely. Can I do that with middleman? Yes and no. I can
certainly get all the output into one gigantic messy ball of javascript in a
single all.js file. Is that maintainable going forward were I to completely
drop the CoffeeScript source? Nope. I've got to find a way to replicate the
dependency reordering logic of middleman and compile all the coffeescript
files individually and rewrite the HAML to generate the appropriate script
inclusion tags. What a pain in the ass. So there's really no time-feasible way
to drop these damn second-class languages that I don't care for.

Middleman also apparently wants to be run and handled entirely by Bundle,
which is some ruby virtual environment manager if I understand it all
correctly, which I probably don't and I don't have enough time to begin to
care about any of that. I have to learn how Bundle works and what it is in
order to just execute Middleman. Should I need to worry about this? Certainly
not.

Does it all work on Windows? Regrettably, no. Or at least not as far as I
tried before giving up and doing it all inside a Linux VM. I tried setting it
all up via MSYS2, my currently preferred Windows development
"sub-"environment, and it all failed totally miserably. Does it work on OS X?
Maybe. Does it work on Linux? Probably. Do I want to use a Linux desktop to
develop? Personally, no.

If this were all done with vanilla HTML, JS, CSS, I wouldn't have any problems
loading it into my preferred development environment, not to mention IDE. That
raises another question... Will my IDE understand all the second-class
languages being used here? Does it recognize HAML or SCSS or CoffeeScript?
Maybe. Will it recognize them as first class citizens with full code-
completion support? There's no guarantee. What is guaranteed is that pretty
much any modern IDE WILL understand HTML, JS, and CSS. They pretty much have
to if you're using them for web development.

## Learning curve

I touched on this a bit in the previous section, but I just don't have the
time to dedicate to learn all these second-class languages. They look
completely foreign to me. My eyes are not trained to notice what is
significant and what is insignificant in the code produced in these languages.
I can't just scan through an unfamiliar language and gleam semantics from it.
Maybe some seemingly inconsequential language sigil completely changes the
expected behavior of the affected code and I'm unaware of that, i.e. maybe
just adding a '@' or '~' sign here or there completely turns the code's
behavior on its head. Maybe they mean nothing and are accepted as part of
identifiers in your language. Maybe it's a mixture where '@' means nothing but
'~' means something very important to the semantics. Do I care? Not right now.

## Wastage

The bottom line that irks me about all this is that there is so much waste
generated as a side-effect of installing all the build tools to take advantage
of these second-class languages' touted benefits over their first-class
counterparts.

I need to install two extra languages (ruby, node) with their own runtimes
that I don't use on a daily basis JUST to run the build tools to recompile
these second-class languages into their first-class counterparts so it all can
be run by a browser. Then I need to install these tools into their respective
runtimes with their own list of per-runtime dependencies.

Does Middleman install the nodejs runtime? I don't have a clue. I might have
two or three different installations of node.js or ruby or their gems or npm
packages sitting around in various places on my system now and I wouldn't have
a clue where they are or how to invoke them.

I've had to install I don't know how many ruby gems just to get Middleman off
the ground. So many libraries and useless extra bits of code completely
irrelevant to the final product. Most of those gems required a C++ compiler
installed to compile some native code for whatever reason. Was I supposed to
know that ahead of time? Nobody told me. So I need a C++ compiler to compile
some part of some random ruby gem that's going to run for maybe 10 seconds as
part of the build process, if it even gets used at all? No thanks. Oh, and if
I didn't actually need it, it's still listed as a dependency and it still has
to be compiled and the gem manager will totally bomb if it can't be compiled.
Can I clean out the object files from the C++ compilation phase or will those
just rot on my disk? What other cruft is ruby installing on my system that I
never asked for?

What a complete waste of disk space and time and energy installing all these
prerequisites. Not to mention energy required to write all the documentation
and list the step-by-step development environment setup procedure for the next
poor sob who has to pick up this project and make a simple change. Do I even
know the minimal step-by-step procedure required to set up the environment?
Nope. I was a blind man groping through a dark alley just trying to resolve
one error at a time. Can I back up two steps ago and undo what I just did?
Nope. Did I record my procedure? Of course not. I just wanted to get the damn
thing off the ground to see what's what.

## Conclusion

You've just ballooned my development environment up by 1,000,000 KB or so just
to compile maybe 300 KB of web code. No, this isn't easier for anyone
involved. Stop doing it. Stop adding unnecessary dependencies to the Nth
degree. Don't make me require two extra big fat runtimes and who knows how
many packages just to run your build tools because they may have allowed you
type quicker. Instead, learn your craft well, and understand it from the
ground up. Do things in the most efficient way possible with the least amount
of total dependencies. Maximize the utilization of the resources of everyone
involved in using your work, not just your own.