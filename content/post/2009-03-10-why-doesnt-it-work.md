---
author: jaymz
categories:
- Uncategorized
date: '2009-03-10T23:15:09'
tags: []
title: Why Doesn't It Work?
---
Debugging, or diagnostics, is a critical skill for any good developer.  It is engineering at its core; problem-solving perfected in art.  There is no <em>One Good Way</em> to diagnose a problem, but I have discovered an excellent process that I employ regularly to almost unfailingly diagnose software engineering problems.

<strong>Mindset</strong>

Too often I hear the question, "Why Doesn't It Work?"  Why do we ask this silly question?  What past experience in our lives has taught us that things should Just Work (TM)?  What fundamental law of nature exists that forces all complex things to behave as we expect them to?  If anything, we should expect and always account for just the opposite!  "What have I done to make this work as I expect?" should always be your first question when things "go wrong."

Perhaps the reason that many developers (in my experience) simply give up on debugging hard problems is the lack of real experience dealing with the class of problem they are trying to solve.  Really, the only way to gain experience with these sorts of things is to get yourself in the right mental state and to just dive in.  When I say, "the right mental state," I mean don't set yourself up for failure.  Don't beat yourself down telling yourself that you cannot solve the problem because it's "too hard" before you even attempt to.

<strong>The Process</strong>

<strong><span style="font-weight: normal; ">As I have discovered, the diagnostic process is simply this:</span></strong>
<ol>
	<li>Ask the right question based on the problem presented</li>
	<li>Detail your assumptions and expectations</li>
	<li>Validate your assumptions</li>
</ol>
We as humans make so many assumptions throughout the course of our daily lives in order to function that it seems almost impossible to isolate and accurately quantify even just a few of them.  We as developers also make a great many assumptions about the systems we are designing or debugging.  All too often, failure to correctly diagnose is the result of not bothering to validate the assumptions that we hold while investigating a problem.  In all my years of debugging, in almost every case of debugging, I have found that one of my assumptions has not held true, and they are usually the most simplistic ones you just plumb forget about or perhaps take for granted that would be so implausible as to be wrong.

Attention to detail is also crucial in investigation.  Do not be tempted to take the easy way out and conclude that a condition you are trying to validate is proven either true or false with weak evidence.  Prove the condition either true or false beyond the shadow of a doubt that there can be no alternative.

To further strengthen a stance, I like to sarcastically state the truthfulness of the antecedent to that stance and expose it for how ridiculous it sounds.  For instance:
<blockquote>Hey, did you know that computers went non-deterministic back in the late '90s?  You might want to look into that!</blockquote>
I hope you're delighting in that.  If not, you either don't enjoy sarcasm, don't know what non-deterministic means, or actually do believe that computers execute instructions non-deterministically.  I seriously hope it's one of the first two.