---
author: jaymz
categories:
- Uncategorized
date: '2014-02-11T18:41:24'
tags: []
title: Fix animated GIF timing
---
Have you found a priceless animated GIF but are disappointed with its timing?
Maybe it's too slow or too fast? I've got an easy solution for you. You don't
need any complex software like ffmpeg or mencoder or imagemagick or any of
that crap (well, they're not crap; they're just ridiculously complicated and
user hostile). All you need is a simple hex editor.

Here's a simple before/after example of what I'm talking about:

Before:

![Before](http://i.bittwiddlers.org/K7L.gif)

After:

![After](http://i.bittwiddlers.org/K75.gif)

As you can see, the top GIF is much slower (depending on your browser, here in
Chrome it animates at the default of 10fps). The bottom GIF is much closer to
the original movie's framerate of approximately 25fps.

To do these kinds of simple corrections, all you need is a hex editor with a
search and replace feature that lets you search for hex strings and replace
them with other hex strings. The venerable [HxD](http://mh-nexus.de/en/hxd/)
is a fantastic hex editor with exactly such a feature. Go download and install
HxD now.

Now download your new favorite timing-challenged GIF to your local computer
and open it with HxD.

Search (CTRL-F) from the beginning of the file for the hex string `21 F9 04 05
00 00` (you need to select Hex String from the drop down; the default is Text
String which is not what you want). If you cannot find any matches using this
6 value string, search instead for `21 F9 04 05` (leave off the last two `00
00`). If even that does not find any matches, then search for just the first
three hex values `21 F9 04`. Whatever you do find, copy 6 values out of the
display starting at `21 F9 04` and put them back into the search/replace
dialog.

The last two values of the 6 value sequence specify the delay time to wait
after displaying each frame. You'll encounter many copies of this sequence
throughout the GIF file because it will exist for each frame of the animation.

Most of the problematic GIFs you'll encounter will just specify `00` for the
frame delay time, meaning "don't care", which is pretty dumb if you think
about it. Browsers will just interpret that as defaulting to something
obscenely slow and useless like 10fps, which explains why the GIF appears to
be slow in playback.

Once you've identified the 6 value sequence to search for, go ahead and
replace all occurrences of it with `21 F9 04 XX 07 00` (where XX is the same
value as what you searched for, which may or may not be `05`). The second-to-
last value, `07`, is the frame delay time measured in 1/100ths of a second.
Feel free to modify that value to your liking. Choosing the best value here
depends entirely on the source material's frame rate, so I cannot tell you
exactly what to fill in here.

I find that useful values are in the range from `04` to `07`. Remember that
the smaller the number, the faster the animation will run. You can do the math
yourself based on the source material's frame rate: 100 / n, where n is the
frame rate.

  * `03` is pretty good for 30fps source content (actual rate will be 33.333fps, a bit too fast and sorta noticeable)
  * `04` is perfect for 25fps source content (most movies)
  * `07` is pretty good for 15fps source content (actual rate will be 14.286fps, a little slow but not very noticeable)

Unfortunately, we can't specify fractional values in this animation delay time
field, only integer values. This appears to be an oversight of the GIF
animation specification. The 16 bits of space reserved for this animation rate
value is horribly underutilized. No one should ever need an animation delay of
655.35 seconds, for instance. They should have instead stored a frequency
value here, not a delay time value. Off the top of my head, I would make use
of these 16 bits to store the animation rate in fps at a x100 scale. This
would give much finer grained control over the frame rate, e.g. storing 2,997
as a 16-bit unsigned integer value will yield a playback rate of 29.97 fps, or
3,000 for 30.00fps, or 1,500 for 15.00fps, etc.