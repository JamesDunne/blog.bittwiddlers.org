---
author: admin
categories:
- Uncategorized
date: '2011-07-06T21:59:14'
tags:
- feather
title: 'Feather: A new language based on asynchrony'
---
I've spent most of my free time over the last few weeks in pursuit of designing a new programming language, one designed for asynchrony from the ground up. I call this language "Feather," in the hope that it will be lightweight, simple, elegant, and just might possibly enable one to fly.

My core goals for this new language are:
<ul>
	<li>asynchronous execution by default with explicit mechanisms to revert to synchronous execution.</li>
	<li>immutable data and no ability to share mutable state between independent threads of execution.</li>
	<li>static typing and complete type-safety.</li>
</ul>
Keeping this goal list very short will allow me to actually achieve all of these goals relatively easily with a final, working reference implementation of a compiler and runtime system.

<strong>Asynchronous execution:
</strong>This is the primary and most important goal of the language. Where possible, functions must be allowed to execute asynchronously. That is, the completion of one function need not depend on the completion of another function. This assumes execution independence of functions. Of course, this will not always be possible in every case, since one function may rely on the computed results of another function in order to complete, creating a dependency. There may also be times where execution of certain functions need to take place in a specific sequence, but in general I believe these to be special cases.

<strong>No shared, mutable state:
</strong>The main problem with allowing just any random set of functions to execute asynchronously with respect to one other has to do with shared mutable state. For instance, if just two functions can modify the same shared state and both are executing asynchronously without any synchronization between the two, the final effects on the shared state are undefined. The simplest solution to this problem is to disallow the sharing of mutable state between functions.

Combining these two goals as core to the language has never been done before in any programming language I've ever used. Sure, other languages support asynchrony and even fewer may support immutable data, but not to the extent of putting them at the core of the language's design. The concept of defining data as immutable is not the same as forcing that mutable data cannot be shared across threads of execution.

I hope that defining these goals up front justifies the language's existence enough for me to further purse its design and development. For once, though, I think I'll end this post before it gets too lengthy. I have much more to talk about in regards to this language and its features than one post can contain. So, until next time!