---
author: jaymz
categories:
- Uncategorized
date: '2009-02-28T22:24:54'
tags: []
title: Memcached Primer
---
Memcached at its core is a simple network service that acts as a hashtable
with an easy-to-use ASCII-based protocol for communicating with it over a TCP
socket. You can even telnet into it and give it commands yourself, but I
wouldn't recommend this for anything but the most simplistic 'stats' and
'flush_all' commands, which can be very handy. It is really meant to have
application clients connect to it (default configuration allows up to 1024
simultaneous connections) and have each client tell it what to do. It doesn't
do anything on its own. I find that it sometimes helps to state the obvious so
as to obliterate any time-wasting misunderstandings :) As I said before,
memcached is basically an efficient and scalable hashtable with support for
atomic get and set operations and various other convenient methods. For
starters, memcached nodes (i.e. an instance of the memcached server running at
a specific IP address and TCP port number) do not communicate with each other
at all. As far as scalability, one node can only handle so much load.
Memcached is best deployed en masse with a pool of nodes. It is up to your
applications to determine how to effectively make use of your pool of nodes
and to distribute load across them all. One common technique is to define a
list of node IP:port combinations and use a (hash value of your key) % (the
number of nodes) to determine which node to access for any given key. As long
as the hash function is consistent (what good hash function isn't?) and only
reliant upon the key's name, then for each key you need to get or set, you
will have exactly one node responsible for that key. For instance, key "bob"
will always go to node #1 and key "jane" will always go to node #2. The
distribution of data cross nodes clearly depends upon your key data and your
key hash function. Consider the following pseudo-code:

    
    
    ServerNode[] nodes = new ServerNode[4] { port 11211, port 11213, port 11215, port 11217 };
    
    public ServerNode ChooseBestNodeForKey(string key)
    {
      int nodeIndex = hash(key) % nodes.Length;
      return nodes[nodeIndex];
    }
    

The downside of this is approach is that the list of nodes must be fixed and
cannot grow or shrink at runtime. Also, if a particular node fails then all
keys that would hash to that node for access will always be cache misses and
the node will have to be restarted or replaced. All these are side effects of
the simple modulus (%) operation. My best advice would be to treat memcached
as what it is: a giant, fast, unreliable hashtable. Do not look here for data
integrity or data replication across nodes or any other such nonsense. It is
simple and it is meant to be used simply. The memcached software itself is
quite reliable; I've never experienced a node crashing due to software errors
in all my many months of (mis)use. The unreliability that I'm referring to
here is the unavoidable consequence of hardware/OS failure when deploying many
nodes on many different physical machines. Another good piece of advice is to
let memcached work for you and to ask it for exactly what you need, no more,
no less. This might seem strange to say, but if you really look deeper and
think about it, it really needs to be said.