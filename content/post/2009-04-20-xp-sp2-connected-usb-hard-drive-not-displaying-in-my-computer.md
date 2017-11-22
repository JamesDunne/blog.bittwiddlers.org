---
author: jaymz
categories:
- Uncategorized
date: '2009-04-20T10:26:15'
tags: []
title: XP SP2 Connected USB Hard Drive not displaying in My Computer
---
Just ran into this lovely issue here...

I connected up a USB HD, a Maxtor BlackArmor, and it wasn't displaying in My Computer at all.

It turns out that under some random circumstance, the Maxtor device was mapped to J: in Disk Management, while simultaneously, a network drive was mapped to the same drive letter by my employer.  The network drive takes precedence and so that is what shows up mapped to J: in My Computer and the Maxtor device will never show up.  Don't ask me why network drive mappings are not checked for conflicts while adding an external device.

The solution was to go into Disk Management and remap the device from J: to some other available drive letter and voila it shows up in My Computer and everyone is happy.  Hope this helps someone banging his/her head against the wall!