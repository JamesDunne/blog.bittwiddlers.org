---
author: jaymz
categories:
- Uncategorized
date: '2013-05-21T17:57:15'
tags: []
title: Home Recording Advice
---
Here's a bit of home recording advice I just gave to a fellow YouTuber. If you
don't know, I have a YouTube channel where I post home-recorded guitar cover
videos [here](http://www.youtube.com/user/jsd1982). And if you do know, good
for you buddy. Anyways, I thought this was a valuable collection of knowledge
I've gained about the subject and summarized fairly well. The question posited
was about where to spend your money to get the most bang for your buck, so to
speak.

Obviously if you want quality you'll need to spend a bit of cash, but there
are places where you can make acceptable trade-offs. Here's where you ought to
spend your money best, in order of importance:

  1. Guitar instrument, guitar strings, and pick (aka plectrum)
  2. Guitar amplifier (if you don't like the sound coming out of your amplifier, you won't like what it sounds like on the recording)
  3. Instrument cables (avoid crackly cabling with poor connectors; Planet Waves is generally good)
  4. Studio monitors (I have Yamaha HS80M pair and HS10W subwoofer, subwoofer is probably optional for starting out)
  5. Recording room treatment (a couple of Auralex foam pads stuck to the wall in strategic locations does wonders)
  6. Microphones ($80 - $100 should suit you fine here, just get a Shure SM57; they're standard workhorses and sound great on guitar speaker cabinets)
  7. Microphone XLR cables
  8. Computer audio interface (I use Roland's [OCTA-CAPTURE](http://www.rolandus.com/products/details/1127) ($800) but there are cheaper variants on that same unit with fewer channels. Check out the [DUO-CAPTURE EX](http://www.rolandus.com/products/details/1248/498))

**Disclaimer** : This is just my list and there's nothing inherently right or
wrong about it. It's just a representation of what value I've learned to place
on things in the chain of everything between your fingers executing a musical
performance all the way to the final captured performance in your DAW suitable
for mixing with.

These investments will all enable you be able to capture the sound coming out
of your guitar amplifier into some computer software, a digital audio
workstation. I'd recommend Cakewalk Sonar X2 since that's what I use and am
most familiar with.

What seems to matter the most to the quality of the final mix is actually what
you do in the mixing and mastering phases. You can completely ruin a good
recording with bad mixing. I know; I've done it too many times. Conversely,
you can't make a good mix with a bad recording. "Get it right at the source"
should be your mantra, where the source is any one of: your fingers on the
guitar, the guitar itself, the amplifier, the speaker, the room the speaker is
in, and the microphone at the speaker, including all cabling involved. I guess
"the source" is considered to be anything in the physical realm that is not a
part of your DAW software that leads to producing the digital track.

I also recommend dialing the amplifier gain down quite a bit while recording.
Most great recorded tones are recorded with significantly less gain than you'd
expect. The real trick to getting a huge guitar sound is in layering lots of
lower gain sounds on top of and next to each other in the mix. Also roll off a
lot of low end, like below 100Hz. That'll clear up the low end quite a bit to
let you have some thundering bass and kick drum down there. Otherwise it'll
get all muddied up and you'll be sad.

Finally, for when you get really into this sort of thing, I'd recommend
picking up a re-amp unit. This unit allows you to record the guitar
performance first and play it back through an amplifier to be recorded later,
when you dial in all your settings just right and like what you hear. This is
what the pros do and I've only just started doing it myself.

One final tidbit is perhaps Windows OS specific, and that is regarding driver
modes for how your DAW connects to your audio interface. In Windows, with a
high quality audio interface, you're likely to have the option for using ASIO
which is an extremely low-latency driver mode that lets your DAW talk directly
to the audio interface without going through the Windows kernel as an
intermediary. This offers huge benefits in terms of latency and CPU
utilization in that the system no longer has to do a lot of extra copying and
processing just to get your audio data to where it has to get to anyway.

You only want to use the true ASIO offering from your audio interface driver.
Don't use the ASIO4ALL driver because that one's a big phony. It won't give
you the true low latency of real ASIO that the manufacturer's driver would.
Now, ASIO4ALL is useful as a compatibility layer if the software you're using
only supports ASIO, but don't expect it to be low latency because it simply
cannot be, by design.