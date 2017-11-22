---
author: jaymz
categories:
- Uncategorized
date: '2012-11-27T00:35:33'
tags: []
title: How I fixed the crash in Doom 3 BFG Edition
---
Merely 10 hours ago, id Software released the GPL source code to Doom 3 BFG
Edition. Unfortunately, when I built the game with VS2012 Premium, the Doom
Classic modes crash (both Doom 1 and 2) instantly. Here is the small tale of
how I [fixed that bug](https://github.com/id-Software/DOOM-3-BFG/pull/1 "Pull
Request"). The obvious thing to do was to fire up the game in Debug mode and
see how far I get. The debugger (under default configuration) wasn't giving me
much when the code bombed out due to an _unhandled_ Access Violation Win32
exception. The key was to force the debugger to break when the access
violation exception occurs in the first place rather than letting it pass
unhandled. VS2012 gives you a check-box labeled "Break when this exception
type is thrown" when the _unhandled_ exception is caught. Turn this on and
restart the game and try to start up Doom 1 or 2 from the main menu. Now we
get a first-chance exception occurring in `r_things.cpp` line 196:

    
    
    intname = (*int *)namelist[i];
    

A quick check to the Locals debugger window shows that `i` is 138. The access
violation exception is thrown by the OS when the process tries to read memory
at `namelist[138]`. Let's try reading from `namelist[137]` using the Watch
window to see if index 137 is safe. Okay, everything looks fine there at index
137. It's just at 138 where it bombs out. Let's remember this number. Now
let's step backwards a bit and try to find our place in the code. Where did
this `namelist` pointer originate from? Jumping back to `P_Init` in the call
stack shows us that `P_InitSprites` was called with `sprnames` and
`P_InitSprites` hands that off to `P_InitSpriteDefs` unchanged. Let's take a
look at this `sprnames` in `info.cpp`...

    
    
    const char * const sprnames[NUMSPRITES] = { "TROO","SHTG",**...<snip>...**,"TLMP","TLP2" };
    

That's it? No NULL terminator there? And there's this constant array size
specifier there: `NUMSPRITES`. Visual Studio tells me that its value is 138.
That sounds familiar... Let's go back and take a look at that function where
our first access violation occurred to see why it's trying to read past the
bounds of the hard-coded array (whose length is 138 elements). We can see that
the size of `namelist` (assigned to `::g->numsprites`) is calculated to be
longer than it should because there is no `NULL` terminator present. That
causes the loop below it to try to access memory beyond what's allowed. Here's
the simple counting code:

    
    
    // line 173 in p_thing.cpp:  
    check = namelist;  
    while (*check != NULL)  
        check++;
    

Perhaps the original developer assumed that the const memory section would be
zeroed out and the counting while-loop would just luckily run into an extra
zero that just so happened to be found just past the bounds of the array? I
can't see why this is a safe assumption to make under any context whatsoever.
Perhaps a random happy coincidence of memory layout and padding made this work
in VS2010? Based on this analysis, it seems obvious to me that these methods
should be passing around the array's known count (`NUMSPRITES`) instead of
trying to calculate it dynamically by scanning for NULL terminators. A quick
search through the code shows me that these functions are only used once from
`P_Init` so this should be a safe change to make. This particular instance of
this class of bug makes me wonder what other instances of this class of bug
are lying around the code elsewhere. I think I got extremely lucky in this
instance and could pinpoint a root cause because the data was hard-coded. I'm
going out on a limb here, but it seems that VS2012 added some extra
protections to make sure that access violations were thrown for access beyond
the bounds of statically-allocated memory regions, which makes me doubly lucky
to find the bug. I'm not sure exactly how they've done that, not being too
familiar with the Windows memory management APIs, but I'm sure there are all
sorts of caveats and gotchas with protecting fixed-size memory regions (page
alignment issues, etc.). I wonder if this bug would reproduce in VS2010, or
any other compiler for that matter... The [pull request](https://github.com
/id-Software/DOOM-3-BFG/pull/1) I've submitted just appends the NULL
terminator to the hard-coded array. From here, the code works great and Doom 1
and 2 start up just fine.