---
author: jaymz
categories:
- Uncategorized
date: '2008-10-22T12:56:24'
tags: []
title: Moving Controls from one ControlCollection to another
---
Beware when writing a loop to move ASP.NET Web Controls from one collection to
another! We're all aware of the classic problem of trying to delete objects
from some collection in order. To be more precise, deleting items from a
collection with ordered storage which compacts its storage after a deletion,
much like List<T> or say, a System.Web.ControlCollection. **What's the
problem?** When you iterate over each element in a collection, you generally
start with a foreach, right? Your first inclination to delete all objects is
to just say something like:

    
    
    foreach (var item in collection) {
      collection.Remove(item);
    }

Right? **WRONG**! You'll most likely (depending on the implementation of the
enumerator exposed by your collection type) get an exception saying that the
container cannot be modified while enumerating. Okay, so foreach is out of the
question here. Your next approach might be something equally as lame as the
above, like so:

    
    
    for (int i = 0; i < collection.Count; ++i) {
      collection.Remove(collection[i]);
    }

Congratulations! You've effectively written the same exact WRONG code as what
you'd have tried with the foreach, except now you probably don't get the nice
exception from the enumerator implementation telling you you're doing
something stupid. Again, this all depends on how the collection is
implemented. So what's wrong with both of these approaches? Why the exception?
When removing items from a collection in a forward iteration order, your
running counter variable (in this case 'i') will be invalid the moment after
you perform the first Delete, assuming the delete operation reorders the
elements in the collection to squash out the empty hole it just created. Let's
say you're using a List<int> with the contents [0, 1, 2, 3, 4, 5]. Try
RemoveAt(0). What happens? Well, your List<int> now contains [1, 2, 3, 4, 5].
Note there are only 5 elements instead of the original 6, and list elements
1-5 have moved up by one index. Compare this with your for i = 0 to Count
loop. See the problem? After you delete the item at index 0, and move on to
index 1, index 1's contents is now the old index 2's contents. You'll
effectively delete only half the items in the list if you try this approach.
Two solutions are as follows:

  1. Always call RemoveAt(0) when iterating through the list, ignoring your counter variable. Since the list is reordering its elements' indicies, always removing the first one should guarantee you to delete all elements in the list, after an expensive series of reordering operations that is.
  2. Iterate in REVERSE order, starting from index Count - 1 moving down to 0.

Back to the ASP.NET ControlCollection example... The main problem here is that
the ControlCollection.Add() method actually REMOVES the control from its
previous container collection and adds it to the new collection. That implicit
removal operation is the crux of the issue. Thanks, ASP.NET team. Some more
clear method names would be better here. So, in order to move Controls from
one ControlCollection to another, you'll need to iterate in reverse over the
original collection and call AddAt(0, item) on the new collection. This will
preserve the original order of the Controls and won't reorder them in a
staggered fashion in your new collection, which was what happened to me.
**Solution:**

    
    
    Control newParent = new HtmlGenericControl("div");
    for (int i = this.Controls.Count - 1; i >= 0; --i)
    {
      wrapperDiv.Controls.AddAt(0, this.Controls[i]);
    }