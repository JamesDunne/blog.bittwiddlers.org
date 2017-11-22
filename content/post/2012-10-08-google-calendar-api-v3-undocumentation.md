---
author: jaymz
categories:
- Uncategorized
date: '2012-10-08T11:11:56'
tags: []
title: Google Calendar API v3 Undocumentation
---
Over the many collective months that I've been working with Google Calendar API v3, I've noticed many undocumented "features" and behaviors. I thought I should sum up my experiences here so that someone else struggling to use this "simple" API would have at least some forewarning of the adventure they're about to embark upon.

The documentation up on Google's site is woefully incomplete. Yes, it lists all the methods and most of the parameters and such, the <em>reference</em> documentation; that's great for starters, but it's the bare minimum amount of useful information. What is <strong>completely missing</strong> is the documentation of <em>behaviors</em> you will encounter, what I call the <em>narrative</em> documentation. Google seems to be very bad about narrative documentation in general.
<h3>Uniqueness Constraints</h3>
What seems to be completely missing from the documentation is <em>uniqueness constraints</em> of objects (or resources, as Google calls them).

For example, an <strong>event</strong> is unique by its <strong>iCalUID</strong> property. Is this important fact mentioned on the event <a href="https://developers.google.com/google-apps/calendar/v3/reference/events/insert" target="_blank">insert method</a>'s documentation page? Not at all. In fact, <strong>iCalUID</strong> is not even mentioned on this page. You have to go to the generic <a href="https://developers.google.com/google-apps/calendar/v3/reference/events#resource" target="_blank">events resource</a> page to find the first mention of <strong>iCalUID</strong> at all. Is the uniqueness constraint mentioned there either? Nope.

While we're on the subject of inserting events, there's also the <a href="https://developers.google.com/google-apps/calendar/v3/reference/events/import" target="_blank">import event</a>, which I have no idea about what it does differently than the insert method, other than that they've declared that an iCalUID is required for the import method. The summary documentation is a useless one-liner: "imports an event." Thanks, Sherlock; that was real helpful.

Furthermore, the only documentation about the iCalUID states that it should be an "Event ID in the iCalendar format." I've found this to be completely untrue. That's probably what the field is intended for, but there is absolutely no format validation for an iCalUID. You can put anything you want in here. (TWSS)
<h3>Colors</h3>
Google Calendar API gives you the ability to assign colors to events and also assign colors to whole calendars. What they don't tell you is that assigning colors to events is <strong>completely useless</strong> unless the calendar the events are contained within is your own personal calendar. In other words, assigning event colors to a calendar intended to be shared with others is pointless. The only control over colors you have in that circumstance is to assign the <strong>whole calendar</strong> a single color specific to the user's calendar list. If you really want colorization of events being shared with multiple users, your only choice is to split events across multiple calendars and assign the colors at the calendar level per each user. And of course, don't forget the uniqueness constraint on the Summary property of the calendars you create!

Also, what they don't tell you about colors is that there is <strong>one global palette</strong> of two kinds of colors: calendar colors and event colors. They <em>do</em> tell you there are two palettes, but they do not indicate whether they are global palettes or user-specific palettes. The two (calendar and event) palettes are not the same palette and an event colorId <em>is not interchangeable</em> with a calendar colorId and vice versa. Why use the same type name "colorId" to refer to two incompatible types? Why not just call one an "eventColorId" and the other a "calendarColorId"? Would that be so hard? To be fair, the documentation does make the distinction but it's not obvious at first glance that the distinction is meaningful.

Furthermore, when Google duplicates events on your behalf (and they do - see the Side Effects section below), they don't necessarily duplicate all properties, including the colorId property.
<h3>Recurring Events</h3>
Creating recurring events is extremely frustrating and fraught with many gotchas and stingers. I don't even want to go into it here; avoid it at all costs if you value your sanity.
<h3>Side Effects</h3>
WARNING! Side effects of regular Google Calendar API v3 usage may include:
<ul>
	<li>Adding email addresses as attendees <strong>copies</strong> the event to the attendees' personal calendars. This creates a <strong>completely different event </strong><strong>with its own eventId</strong>, unrelated to the one you created via the API. As far as I can tell, there is no programmatic way to determine if this duplicated event originated from the event you created via the API.</li>
	<li>Deleting a user which owns calendars that are shared with other users will create a private copy of the shared calendars in each users' calendar list and will only delete the original calendars owned by the user being deleted.</li>
	<li>Deleting an event causes it to be marked as a dual "deleted/cancelled" state. I simply cannot figure out the difference between deleted and cancelled, if there is one.</li>
	<li>Trying to re-create a previously deleted event will cause a 409 Conflict response. You must instead resurrect the deleted/cancelled event which has the same uniqueness properties as the one you are trying to create (e.g. the iCalUID must match).
<ul>
	<li>When fetching the event list for a calendar, always set the "showDeleted" parameter to true. This way you can detect if you're trying to recreate an already existing yet deleted event.</li>
</ul>
</li>
</ul>
<h3>Types</h3>
<div>
<ul>
	<li>/events/list accepts timeMin and timeMax arguments and these are simply stated as accepting a 'datetime' argument. Of the myriad possible standardized date-time formats, I have discovered that this value should be a UTC date-time (with offset explicitly set at 00:00) formatted as RFC3339 (yyyy-MM-ddThh:mm:ss.sss+00:00).</li>
</ul>
</div>
There are many more issues than I can list here, but these are fresh in my memory.
<h4>Revisions</h4>
<strong>UPDATE (2012-10-14):</strong> removed the bit about calendars being unique by Summary as that was not true.

<strong>UPDATE (2012-10-19)</strong>: added Types section to document timeMin and timeMax arguments