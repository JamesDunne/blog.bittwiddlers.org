---
author: jaymz
categories:
- Uncategorized
date: '2011-05-27T12:33:57'
tags: []
title: RabbitMQ .NET client API is terrible (rant)
---
I've seen some pretty poorly-designed APIs in my time, but RabbitMQ's .NET client API has got to be among the top worst that I've seen lately. Specifically, I'm talking about the RabbitMQ.Client library that appears to be officially sanctioned by the RabbitMQ project and "documented".

The very first problem you'll quickly see when you get into using this "API" is that everything (e.g. return values and parameters) is typed as a <strong>string</strong>. Strings are problem enough in and of themselves, so why exacerbate the problem by requiring them <em>everywhere</em>?

Let's dive in to this API and look at a few examples in the IModel interface:

void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary arguments);

See that 'type' parameter? What does that mean? What is an exchange 'type' in this context? Can I create my own types? It turns out, no. This string value is expected to be one of ("direct", "fanout", "topic"). How do we know this from the method signature? We don't! Not a clue. No, we have to pore over the user guide "documentation" to find that we're expected to use one of the ExchangeType class's const string members here.

Here would be a perfect candidate for an <strong>enum</strong>. This is essentially an untyped string "enum", primarily used when calling ExchangeDeclare function. I haven't done a full search of the API, but I'd want to say that this is probably the only place these values are used. Not to mention this ExchangeType "static" class isn't even declared as static. Sure, you can new up an ExchangeType! What would that get you? Nothing. Not only did they do it wrong, they did it wrong the wrong way.

Continuing on, the <strong>ExchangeDeclare</strong> method returns `void`, like most methods in this API. This is also terrible in that it gives you absolutely no information back as to the success/failure status of the operation; that would be the bare minimum expectation for a reasonably dull API. What would be preferred is a more complex type system that propagates contextual information through its methods. Now there are some good situations to use void-returning methods in an API, but given this context, I don't think this is one of them.

Why is the void-returning such a big deal? It gives you no information as to what operations should logically follow next after "declaring" an "exchange". The API designer has expectations on how you, the user, should use this API and has documented elsewhere that the expected pattern of invocation is ExchangeDeclare, QueueDeclare, and QueueBind, but you would never be able to discover this yourself without significant trial-and-error at runtime.

The API itself should expose strong hints as to how it is designed to be used, not the documentation. Documentation should be used to clarify what and/or how a specific piece of functionality works/does. The API should scream at you (via its types) how it is to be used <em>correctly</em>. Documentation should scream at you how it should NEVER be used <em>incorrectly</em>.

What would have been nice is if this ExchangeDeclare method gave back some semblance of an Exchange type that has its own methods, maybe something like `QueueBinding BindQueue(Queue queue, ...)`. That also implies there is a Queue type returned from QueueDeclare. All of this typing information should let you logically come to the conclusion that, "Oh, I need both a Queue and an Exchange declared first, and then the Exchange lets me bind queues to it!"

Instead, what we're given is three seemingly independent methods defined on `IModel` (which is a terrible name for something that's supposed to mean a channel, BTW) with void-returning semantics that accept only string-typed parameters and the order of operations is left undefined for you to guess at.

There is also absolutely no XML parameter documentation in this API either, which is more-or-less a de-facto standard in the C# world. Now, there <em>are </em>method &lt;summary&gt;s in the XML documentation, but the parameter documentation is entirely missing. Intellisense would have significantly helped out the discoverability of this API has there been any XML documentation on the parameters. Going back to ExchangeDeclare, at least a NOTE saying that this string 'type' parameter expects its values to come from the ExchangeType class would have been more useful than saying nothing whatsoever.

string QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary arguments);

Why would you use the pre-generics IDictionary type with absolutely no explanation as to what types it expects to see in the dictionary, what it is used for, or even if I can safely pass in a null reference if I don't care right now about extra 'arguments'? There are MUCH better strongly-typed ways of conveying optional information to an API programmatically than using an IDictionary. This should simply not be done, no matter if it be with the pre-generics framework types or with the generic ones (i.e. IDictionary&lt;TKey, TValue&gt;).

What does this method return? A string? Seriously? What do I do with that? What information is in this string? What other methods are looking for this particular string value? This return value is in its own domain, and I would hope obviously not to be confused with any other `string`-typed parameters in this terrible API. I should NOT have to run the program (in which I don't even know if I've done anything right so far) in order to discover what different domains of string values there are and where each one is expected. This is the entire point of a typing system, whether it be dynamic or static. At least with a dynamic typing system there is some structure around your data. Here we just have strings, the lowest of the low.

void QueueBind(string queue, string exchange, string routingKey);

Now we have a new 'routingKey' string parameter. What exactly is the proper formatting of this key? Should I care? Are there any special characters reserved? Why not wrap this up in a RoutingKey struct, at the very least, that's responsible for constructing only-valid routing keys? Nope, we're completely on our own to create valid routing key values that should hopefully conform to the established AMQP standard's definition of a routing key.

string QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary arguments);

We can declare queues! Great. Any restrictions on the queue name? Is that validated by this API? Again with the IDictionary crap? At least we have some bools now, and they're not `string`s whose valid set of values would be ("true", "TRUE", "false", "FALSE") but not "True" or "False". That last part was a joke, but I certainly wouldn't be surprised if that were the convention here too. Why not be consistent and just STRING everything? That was also a joke. I hope you're still with me here. Still, boolean parameters are generally frowned upon.

string QueueDeclarePassive(string queue);

Okay, now apparently there are 'passive' queues. I guess that passive queues cannot be defined as durable, exclusive, or autoDelete. What, no extra IDictionary of arguments this time? Does that mean I declare 'active' queues with QueueDeclare? What's the difference?

All these strings everywhere bring me to another point: case-sensitivity. Is "MYQUEUE" different from "myQueue"? Are routing keys case-sensitive? Also, what if I want to use a non-ASCII character in my queue name, like "ಠ_ಠ"? Will that wreak havoc with this system? It seems this is the only face I'm able to make towards this API's design. Hmm... what if I have some extra whitespace in my string? Are all string values Trim()ed, or just some, or none?

<strong>Summary:
</strong>I generally don't like writing public rants, but it seems no one else has had anything negative to say about this API yet. I feel dumber having used this API. I award the designer no points, and may God have mercy on his/her soul. This makes me not want to use RabbitMQ at all.