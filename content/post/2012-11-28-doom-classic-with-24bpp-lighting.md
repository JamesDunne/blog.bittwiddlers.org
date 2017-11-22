---
author: jaymz
categories:
- Uncategorized
date: '2012-11-28T19:30:58'
tags: []
title: Doom Classic with 24bpp lighting
---
It's been a while since I pulled an all-night coding binge, but last night
that counter was reset to zero. The [fruits of that labor](https://github.com/id-Software/DOOM-3-BFG/pull/2) are a modestly improved look to the Doom
Classic modes under the Doom 3 BFG edition which was recently open-sourced.
Here's a before/after screenshot pair demonstrating the improved colors for
lighting (click for full view): [![](/images/2012/11/8bpp-vs-24bpp-1024x364.png)](/images/2012/11/8bpp-vs-24bpp.png) It takes a keen eye to spot
some differences, but the effect should be apparent overall while playing the
game for an extended period of time, especially while visiting darker areas
in-game. Take a close look at the entryway on the left side and also at the
brighter brown wall on the right side. The Doom Classic modes under BFG are
simply ports of the original Doom engine, complete with the old software
renderer. It seems they patched up the renderer to scale the original
resolution of 320x200 up by a factor of 3x to 960x600. The main game engine
(doom3bfg.exe) simply takes the 8bpp palettized framebuffer rendered each
frame from the DoomClassic library and updates a texture with its contents, to
be presented to the user in the main game window. While I was perusing the
code, I found, by happenstance, this `typedef byte lighttable_t;` line with
these comments above it:

    
    
    // This could be wider for >8 bit display.
    // Indeed, true color support is possible
    // precalculating 24bpp lightmap/colormap LUT.
    // from darkening PLAYPAL to all black.
    // Could even use more than 32 levels.
    typedef byte lighttable_t;
    

This looks like a conversation between developers via code comments (with my
own edits to fix spelling), but the way they did the import to git caused all
authoring history to be lost, probably on purpose, so we don't know who's
talking to whom here. Regardless, what they're saying here is essentially that
`lighttable_t`, which is used to store palette index lookups based on light
levels, could be made to be larger (e.g. 32 bits) to support true color (24bpp
with no alpha), with a few additional code changes to generate said light maps
and look up the raw RGB colors instead. The way the engine works is that there
is a 256 color palette stored in the main IWAD file in the `PLAYPAL` lump. All
textures and sprites in the game data refer to colors in this main palette.
However, there is lighting to be taken into consideration. The engine has to
darken the colors referred to in textures and sprites according to the
surrounding light level and z-distance. This is done with a light map, from
the `COLORMAP` lump, which is simply an optimized palette lookup table for 32
distinct light levels. Each light level has a 256-entry lookup table which
tells it which color from the 256 color palette best matches the original
color darkened to the light level. Of course it won't be perfect since there
are only 256 colors able to be displayed on the screen at one time, so you'll
get some color shifting effects and other quantization effects here. But
overall, the result is rather impressive for 1994-era technology! What I've
done is (mostly) removed the need for the `COLORMAP` lump and gone straight to
calculating the raw RGB colors from the `PLAYPAL` palette based on the light
levels. This way you get direct 24bpp color from the engine. Of course, our
colors are still limited to what's available in the original palette so the
source material hasn't changed, only our rendering is improved. The light
levels available are from 0 to `NUMCOLORMAPS-1`, where `NUMCOLORMAPS` is 32.
According to some comments in the code, light level 0 is full brightness and
level 31 is full darkness. I was able to easily increase `NUMCOLORMAPS` from
32 up to 64, giving more distinct colors and a smoother lighting look. I was
not able to increase `NUMLIGHTLEVELS` though; there's something crazy going on
with the code related to that constant. The part that made this all
(relatively) easy was that the `neo/framework/common_frame.cpp` code which
projects the 8bpp screen to the 32bpp texture is very simple and does the
palette lookup itself. I left this code mostly the same, except I changed the
`screens` array to store larger integers instead of `byte`s. I extended the
`XColorMap` array from 256 entries to `256 * NUMCOLORMAPS` entries which
essentially makes it a larger palette of 16,384 colors instead of just 256
colors. I modified the `I_SetPalette` method to precalculate all the 16,384
colors based on the original 256 colors. The rest of the work involved making
sure that all the rendering code could handle a wider screen element integer
size than `byte`. There were lots of hard-coded assumptions that the element
size would be a byte, apparent in several `memcpy` and `memset` calls. I did
encounter some problems that didn't allow me to fully skip loading the
`COLORMAP` lump. The primary problem was with the fuzz effect for spectres and
your gun (and also other invisible players in network mode). The problem is
that the effect uses a specific colormap (#6) from the `COLORMAP` lump to
"dither" the onscreen colors, which produces an effect that isn't easy to
reproduce with a simple calculation. After failing twice or thrice to
reproduce this effect, I finally resorted to just bringing back the original
`COLORMAP` and doing a little bit twiddling on the `colormapindex_t` values
read from the screen to keep the light levels consistent. The other problem
was the inverted color effect (only used when the player picks up an
invulnerability sphere). I just had to import the colormap at index 32 from
the lump to get this to work and also update the `INVERSECOLORMAP` to be
`NUMCOLORMAPS` since it's now 64 instead of 32. Just a little table
translation there. There appear to be two extra colormaps in the lump that
I've not accounted for so I'm just ignoring them. The game plays and looks
great now. Admittedly, the red- and green-tint effects don't look as good as
they used to for some reason. I'll have to check that out. The effect comes
across, but it gets too dark further in the distance.
