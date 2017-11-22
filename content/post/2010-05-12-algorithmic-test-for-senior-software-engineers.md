---
author: jaymz
categories:
- Uncategorized
date: '2010-05-12T12:53:20'
tags: []
title: Algorithmic Test for Senior Software Engineers
---
<a href="http://bittwiddlers.org/wp-content/uploads/2010/05/g2p1.png"><img class="alignnone size-full wp-image-88" title="Algorithm Test" src="http://bittwiddlers.org/wp-content/uploads/2010/05/g2p1.png" alt="" width="786" height="476" /></a>

Having had enough of the tired old "ask a question get an answer" interview tactics, I decided to put together a test bed application to test the interviewee's real-world problem solving skills. Based on feedback from other team members, I did not actually force this upon on any of our recent interviewees.

The purpose of the test is to find out if the interviewee can actually develop an algorithm to solve a problem from scratch and can fit it into an existing project. Another critical skill to test is how well the candidate can read specifications and more importantly how attentive to detail he/she is.

This test is based on an algorithm I developed long long ago to convert game maps from Wolfenstein 3-D to DooM 2(which was quite a success but I unfortunately have lost the code since then). The two games are based on very different technologies and the ways in which each stores map information is completely different. Wolf3D stores its maps as a simple 2D square tiled grid filled with block type numbers where the number represents a specific wall texture type and sometimes an implied function like a door. DooM (and DooM 2) store their maps as a set of convex polygonal sectors with a floor and ceiling height. A sector is made up of a set of line segments with special properties applied to one or both sides of the line segment.

The algorithm that the candidate is tasked to implement must convert the incoming 2D tiled grid filled with solid/empty values into a set of line segments which represent the inner edges of the walls found between a solid and an empty tile. I chose to simplify the requirements to deal only with solid/empty tiles instead of 100-or-so different tile types.

Here is the algorithm's method signature:

IEnumerable&lt;LineSegment&gt; TransformBlocksToLineSegments(RectangularMap map, Point startingPosition)

There are many ways to implement the algorithm and many detail points to consider:
<ul>
	<li>The order of beginning/ending points for the line segment implies the direction that the line segment follows. In DooM, this ordering becomes very important for determining which side is the front vs. back. An enclosed sector must have all its line segments pointing in a clockwise order, i.e. the start of one line segment must point at the start of the next line segment and so on until the end of the last line segment points to the beginning of the first line segment.</li>
	<li>Consider allowing/disallowing orphaned areas that the player can/cannot reach from the starting point. A recursive flood-fill implementation of the algorithm will skip over orphaned areas whereas a nested-for-loop approach will include all areas, regardless of reachability.</li>
	<li>Joining line segments per each tile together into the longest-running line segment can be challenging. The simplest case to implement is to create a single line segment per tile's edges and leave it at that, but that's very wasteful in terms of number of line segments.</li>
</ul>
There are 6 sample solutions implementing the algorithm using various combinations of these points to compare the candidate's implementation against. The test bed application also includes a benchmark feature which compares the averaged run times of each implementation.

The intent of the test is a take-home problem where the candidate can spend as much or as little time as he/she likes. The 6 sample implementations are included in a binary-only assembly form, but this is easy to defeat with a tool like Reflector.

The fun part is the ability to draw out rectangles on the grids and to see the live results of the selected algorithm and how it reacts to the new information in the map. Left-click-dragging will create an empty rectangular space and right-click-dragging will create a solid rectangular space.

If you are planning to use this test, be aware of cheating. Have the candidate explain their implementation to you and be sure they're not just copying someone else's solution and taking credit for the work.

Download here:
<ul>
	<li><a href="http://bittwiddlers.org/wp-content/uploads/2010/05/B2P-20100520-Interviewee.zip">B2P-20100520-Interviewee</a> copy to give the interviewee candidate</li>
	<li><a href="http://bittwiddlers.org/wp-content/uploads/2010/05/B2P-20100520-InterviewerAnswerKey.zip">B2P-20100520-InterviewerAnswerKey</a> copy to keep for yourself - includes sample solution code</li>
</ul>