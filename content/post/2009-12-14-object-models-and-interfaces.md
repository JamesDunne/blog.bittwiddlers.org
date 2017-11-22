---
author: jaymz
categories:
- Uncategorized
date: '2009-12-14T19:26:29'
tags: []
title: Object models and Interfaces
---
Whatever I design I've noticed that I always opt for flexibility first, followed closely by simplicity, performance, and maintainability (in no particular order of preference). Allow me to share with you a recent product of my own design process as it applies to n-tier architecture for solving general business problems.

In this proposition, dear reader, allow me to assume that you have at least a passing familiarity with n-tier design. Also please let me assume you have first-hand experience in working as a developer within a team striving to develop and complete a software product. Furthermore, please allow me the courtesy of assuming you have experienced the chaos evident in the implementation of such a product where each developer on the team is given free reign to add/remove/change the object model, its behaviors, use of design practices, calling conventions, data-access strategies, etc.

This design proposal will concentrate mostly on methods of efficient data retrieval, the reduction of redundancy, and reusability of business logic code such that it is not tied directly to the method of data access. No attempt is made to design for any sort of data schema abstraction.

Since my primary language of interest is C# (specifically v3.5 running on the .NET framework), the code examples will reflect this preference, but hopefully the design approach should be able to be generalized to other languages/frameworks.

Let's start with some terminology. I use the term `domain model` to represent some logical concept specific to one domain. I may interchange this to mean the logical representation as well as the object type used to represent that logical information in my implementation. For instance, a `Student` domain model consists only of the properties directly related to a student of a college. The details of his/her financial aid status, enrollment status, gpa, etc. are all irrelevant as far as the basic model of the student is concerned. These other pieces of data should be represented by other domain models *related to* the student, but not be represented as properties *of* the student.

I have just introduced the first constraint of this design. More formally, domain models must only contain properties which represent information directly related to the object the domain model represents. In your implementation, it follows that the domain model should be no more complex than a simple class or data structure (with reference semantics, not copy semantics) containing only immediate data. This constraint is intended also to preclude the possibility of implementing so-called lazy-loaded properties, which when accessed go to perform some data retrieval.

The second constraint is such that the implementation must make it clear what is being done by invoking a method, accessing a property getter, constructing an object, etc. This further reinforces the no-lazy-loads case mentioned in the first constraint.

An interface (or your language's effective equivalent which enforces implementation of a pattern of method signatures and contracts) must be defined for each domain. Let us call this interface `IDomainRepository` in keeping with generally accepted C# naming conventions.

In this interface should be rather simple definitions of the form `MyModel GetMyModel(MyModelID id)`, assuming your data schema for MyModel employs an identity column. Other basic relational query forms represented as simple method names can be defined on this interface.

For data retrieval scenarios which retrieve multiple domain model instances, make the return value type which represents the collection as an enumerable type. In C# 2.0 and above this would be IEnumerable&lt;T&gt;. A bad idea would be to expose a specific implementation of an enumerable data structure such as List&lt;T&gt;. The reasons for this will become clear later if they are not already.

Our third constraint is that the complexity of data query methods on the `IDomainRepository` interface must be such that the domain model instance(s) can be retrieved with a simple projection from the underyling data store. The method's implementation must be able to retrieve the data given only the parameters it was called with.

Let me remind you that we are defining an interface here. We have not implemented any actual code yet. The point of the interface is to allow variation in implementation details, composability, testability, separation of concerns, and a whole host of other benefits too numerous to enumerate in this post.

Assume we were to implement a rather naive implementation of this interface using your favorite ORM tool which maps your underlying data schema onto your domain models. This would be a complete enough low-level API to get the data that you want and be able to write effective business logic using simple GetXByID(id) calls. Obviously it would not be very efficient with each call blocking to go get data from the database.

Now let us define a second, independent interface which we could consider a sister to the first `IDomainRepository` interface. Let us call this second interface `IDomainQueries`.

Remember, the interface definitions are separate, but that certainly does not mean that the class(es) which implement(s) them has to be. In fact, we want a single class to implement both interfaces.

For the second interface, define methods which handle your most common data queries involving joins and the like. These can be as specific as you like. The result of a query method should be a reference to a base domain model from which all other retrieved data can be accessed via the domain model graph.

This domain model graph can and will be traversed by consumer code invoking the basic GetXByID(id) methods of `IDomainRepository`. The implementation of both interfaces combined into the class can be assumed to work in tandem. The implementation of `IDomainRepository` should no longer be assumed to always do a naive direct-to-database retrieval. Of course its fallback behavior should be to retrieve from the database but the most efficient implementation should first check a local hash table for a domain model reference given the identifier. This local hash table is assumed to be populated by methods implemented from the `IDomainQueries` interface.

Consumers of the implementation classes should assume the lifetime of the implementation class and all domain model instances created by that instance are limited to a single unit of work, whether that be a web request, service request, or whatever else consists of a sequence of code called to perform a single action to retrieve a consistent view of data. An antipattern would be to use a static or per-thread instance shared across multiple units of work or requests, whatever your context may be.

The consumer of the implementation may write code first making use of only methods of the `IDomainRepository` and optimize it later by adding perhaps a single call to a `IDomainQueries` method which optimizes data retrieval. This can be done since the implementation of the `IDomainRepository` interface methods can be made to work with or without the presence of the `IDomainQueries` interface.

While you could stop here and implement this design with just the database retrieval implementation, you would be missing out on the composability of the design! You can implement a separate caching layer which can intercept your method calls and upon cache miss delegate to the database retrieval code. You could even implement a testbed implementation serving up hardcoded data or mocked using an object mocking framework. The possibilities are virtually endless. Just remember that composability is key here.