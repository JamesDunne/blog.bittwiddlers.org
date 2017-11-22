---
author: admin
categories:
- Uncategorized
date: '2011-10-03T22:35:10'
tags: []
title: IVO-CMS - Part 2 - The New
---
Any good system architecture is based on the concept of layering. A basic premise of layering is that one layer should not concern itself with the details of any other layer. With the proprietary CMS described in Part 1, my failure to realize that fact was the critical design and implementation flaw of the system. The implementation of the system is obsessed with the revision control part. However, with IVO-CMS, I've designed the content management system aspect to be ignorant of the revision control system. Think of it as a CMS wrapped in a revision control system. The entire CMS system itself can still function with a non-versionable implementation of the revision control system.

This is possible because the meat of the revision control system is simply a file system that stores blobs. Virtually any system can be designed with such an organization mechanism. The contents of the blobs and their relation to one another are what the design of the CMS is concerned with.

We're no longer limited by the relational schema of a database. Our data structures are simply serialized to and from blobs stored in the revision controlled file system.

IVO-CMS uses XML as its serialization formation in its blobs. This is a natural choice because of the ability for an HTML5 document to be serialized as XML. HTML is our primary output for a web content management system, so a clean ability to manage and output it must be central to the design of the system.

IVO-CMS does not define any traditional CMS concepts at its core. Things like "page", "content", "navigation", etc. are never mentioned. At its core, IVO-CMS is simply an HTML renderer with an extensible content processing engine.

The most basic concept at play is the blob, for lack of a better term. A blob is the recipe, written in XML, for rendering an HTML document fragment or even a complete document. IVO-CMS's blob maps directly onto IVO's blob.

The content rendering engine for IVO-CMS simply starts up a streaming XML reader on a blob and copies the XML elements read directly to the output, with a defined path for handling custom elements.

All custom processing elements of IVO-CMS start with 'cms-'. The most basic processing elements built-in are:
<ul>
	<li>cms-import</li>
	<li>cms-import-template
<ul>
	<li>cms-template</li>
	<li>cms-template-area</li>
	<li>cms-area</li>
</ul>
</li>
	<li>cms-scheduled</li>
	<li>cms-conditional</li>
	<li>cms-link</li>
</ul>
Any XML element that starts with 'cms-' is sent to a pluggable provider model to be parsed and handled.

Let's start with <strong>cms-import</strong>. When a cms-import element is found in a blob, it should have the form &lt;cms-import path="/absolute/path/to/another/blob" /&gt; or &lt;cms-import path="../relative/path/to/../../another/blob" /&gt;. Both absolute and relative paths are allowed to describe the location of the blob to import. The imported blob is sent through the content rendering engine and its output is directly injected into the output of the currently rendering blob. The relative path is relative to the currently rendering blob's absolute path.

An imported blob must be a valid document fragment with one or many root elements that are fully closed. In other words, it cannot contain any unclosed elements which makes its usefulness in rendering partial HTML content limited. This is why <strong>cms-import-template</strong> was invented.

Think of cms-import-template as importing a template which has areas that can be overridden. This is analogous to the Page/Master Page concept of ASP.NET's web forms. The page is the currently rendering blob and the master page is the imported template blob. Only certain blobs may be imported as templates - those that contain a single root element: <strong>cms-template</strong>. Unlike ASP.NET's web forms, multiple templates may be imported into a single blob and templates may even import each other.

The <strong>cms-template</strong> blob may contain templateable areas with <strong>cms-template-area</strong>, uniquely identified with an 'id' attribute. The blob importing the template may override these template areas' contents with a <strong>cms-area</strong> element and an 'id' attribute that matches the template. The order the cms-areas are defined in is important since all XML elements are processed in a streaming fashion and there is no back-filling of content.

Now we come to <strong>cms-scheduled</strong>. This is an element that allows part of a blob (or the entire thing, if so desired) to be rendered on a scheduled basis. It must first contain some &lt;range from="date" to="date" /&gt; elements that define the date ranges when the &lt;content&gt; element should be rendered. An &lt;else&gt; element may also be present to render content for when the current date/time does not fall into any of the date ranges.

Next up is the <strong>cms-conditional</strong> element which can primarily be used for selectively targeting content to specific audiences. It presents the content author with a simple system of if/else-if/else branching logic to determine which content to render for whichever audience. The inner elements are &lt;if&gt;, &lt;elif&gt;, and &lt;else&gt;. The attributes on the &lt;if&gt; and &lt;elif&gt; elements make up the conditional expressions.

The system evaluates the conditional expressions (a dictionary of key/value pairs pulled directly from the element attributes) to a single true/false value by using a "conditional provider" class. This class may be a custom implementation provided by the site implementer since it is best left up to him/her to define exactly how audiences may be defined and evaluated based on the user that the content should be rendered for.

However, that may be asking too much of the site implementer because it would potentially involve defining a domain-specific language for evaluating expressions. I may provide a default implementation that allows for defining complex boolean logic expressions, e.g. &lt;if expr="(role = 'manager') and (dept = '23')"&gt;Hello, managers in dept 23!&lt;/if&gt;. The values of variables 'role' and 'dept' would be provided by a provider model implementation that the site implementer could more easily develop.

Finally, the <strong>cms-link</strong> element is responsible for allowing the content author to simplistically create anchor links (i.e. the &lt;a&gt; tag) to other blobs without having to worry about the details of how the URL gets mapped to the referenced blob. This is primarily for SEO purposes so that you don't have to force an implementation of your URL rewriting scheme into your site's content. The site implementer can write a custom provider that takes the linked-to blob's absolute path and rewrite it into a URL that should pull up that blob as its own page or as a wrapped article page or whatever other linking scheme he/she wishes to implement. This lets your content be internally consistent without worrying about URL details. Changing your SEO strategy for your content should be as simple as rewriting the link provider.

Now that we have the nitty-gritty details on how the content rendering engine and its basic processing elements work we can talk about how such a low-level engine can be integrated with an existing site, but that's for next time!