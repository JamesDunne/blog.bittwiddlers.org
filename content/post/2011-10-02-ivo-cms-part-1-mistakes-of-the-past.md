---
author: jaymz
categories:
- Uncategorized
date: '2011-10-02T22:54:25'
tags: []
title: IVO-CMS - Part 1 - Mistakes of the Past
---
For the first time, I'm starting a series of blog posts. This series will focus on a web content management system (IVO-CMS) that I'm currently designing and developing, partly out of curiosity to see if it can be done, partly out of the fun in implementing something new, and partly out of a need to correct my own mistakes of the past. This first post will explain the past mistakes in designing an existing proprietary CMS for my current employer; one you've never heard of, never seen, and never used due to its proprietary nature. I will tell you all this in order to give you context around how the design of the new system was influenced by the failures of the current one.

Let me start out by describing the general system architecture and then demonstrating the weaknesses of such a system. Ironically, this blog post is probably the most accurate documentation of this system's architecture that currently exists.

Since the system allows each piece of content it tracks to be revision-controlled, I started out by designing a rather simplistic revision control system.

Each versionable object domain is represented in the database by a pair of tables: the "<strong>versioned root</strong>" table and the <strong>version</strong> table. What I have termed the "versioned root" is nothing more than a conceptually meaningless container that keeps track of the currently published version record by foreign key reference and is used to tie all the versioned records together by a common ancestor/container. Its basic schema is (VersionedRootID int PK, PublishedVersionID int FK, IsPublished bit).

The version table is what contains all of the versionable data that may change over time and have a history kept track of. Its basic schema is (VersionID int PK, VersionedRootID int FK, ProjectID int FK, all other data columns that are versionable).

For instance, let's say we have XHTMLContentFragment (versioned root table) paired with XHTMLContentFragmentVersion (version table). The XHTMLContentFragmentVersion contains all of the useful system data and the versioned root table is there to tie all of those version records together and assign an identity to each versionable content fragment object.

A project is a container of versioned items; all versioned records point back to a specific ProjectID. Creating a project must be done first before creating any new versions. Think of it as a "changeset" that's created ahead of time to associate a set of changes that are going to be applied simultaneously. A project is what gets published, and doing so simply bumps all of the PublishedVersionID FKs on the versioned root records up to the VersionedID that is contained in the to-be-published ProjectID. All of this is done in a transaction so as to be atomic. The idea being that the entire live site gets updated at once and that it all goes live or nothing goes live.

In my eagerness to implement this design, I neglected to account for a few implementation details.

Firstly, with this schema, a versionable item is not able to be deleted from the system in an atomic fashion by doing a publish. I did not implement an IsDeleted bit on the versioned root table to keep track of which objects are still alive. Furthermore, I failed to implement a companion table that records the publish history. There is no way to know what previous VersionID was published before the current PublishedVersionID.

Furthermore, the versioned root concept is flawed in that these containers are meaningless. They do not lend themselves to performing cross-system content merging. In fact, they actively hinder such a useful action. Imagine you have several database environments, like development, testing, staging, and production. Content changes in production would need to be merged down into development so that developers have the latest content to edit for system integration purposes (adding links to new features, etc.) while the production content maintainers can continue to make content changes to the live system.

The reason these containers are meaningless is primarily because of the choice to use auto-incrementing database identity values as primary keys. VersionedRootID 132 has no meaning to anyone except the actual versioned records that point back to it as their container. Its identity value 132 represents nothing semantically useful to the system. If I try to merge content from one system's VersionedRootID 132 to another system's VersionedRootID 132, that means nothing. I cannot know that those two containers are the same across the two independent systems.

Finally, the nail in the coffin is that I failed to track the parent VersionID when creating a new version. The system simply copies the content from the currently-published version record and makes a new version record with it then lets the user edit that. The lack of a recorded parent VersionID means that there is no way to tell what was published at the time of object creation. Projects may be created by any user of the system at any time and there may be multiple active projects being worked on simultaneously. Without an accurate history, there's no way to figure out how to accurately merge this project's change with another project's change to the same versionable object. For instance, project B gets created based off published version A and project C gets created based off published version A as well. Let's say that project B gets published first. This means project C is still based off published version A, and not version B. If project C gets published, it effectively overwrites changes that were made in project B. Without recording the parent VersionIDs, we can have, at best, a baseless merge.

In summary, with this CMS we have a weak history in that all the previous project versions are stored, but there's no way to know in what order each project was published in. We also don't know whether content from one project was accidentally overwritten with changes in a second project that was created before that first project was published but was then later published. There is no way to delete versioned items. There is no clean way to merge content across independent systems/environments because an accurate history is not kept and the container structure for the versioned items is meaningless.

All of these problems got me thinking about a better solution. That solution came, somewhat surprisingly, with <strong>git</strong>... but that's for next time!