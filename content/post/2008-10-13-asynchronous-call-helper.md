---
author: jaymz
categories:
- Uncategorized
date: '2008-10-13T22:07:40'
tags: []
title: Asynchronous Call Helper
---
Have you ever gotten lost in all your BeginInvoke, EndInvoke, callback delegates, and IAsyncResults using the .NET Asynchronous Invocation Pattern?  Is it just too much code to write in order to execute a simple task?  Is there a better way?  Yes.<!--more-->To solve this little problem and also to help me write more efficient,
scalable, and _readable_ server-side code I created the handy wrapper/helper
class [AsyncHelper.cs](/images/2008/10/asynchelper.cs "AsyncHelper"). **NOTE:** The default
configuration of the class depends on Mike Woodring's custom
[ThreadPool](http://www.bearcanyon.com/dotnet/#threadpool). It can easily be
switched to use the default .NET framework System.Threading.ThreadPool by
disabling the UseCustomThreadPool conditional compliation switch, but I would
recommend against it for long-running asynchronous operations. I encourage you
to do your own research on the subject for more information as to why this is
a bad idea. This idea came to me while I was working on a web site project
whose home page needed to do 4 or 5 independent WCF service calls to get data
to display on various components. Since none of the data pulled back by any of
the service calls depended on each other, I wondered why all the service calls
were waiting one one another. **Details** The idea popped in my head to simply
tie an asynchronously executed method with a callback method that processes
the first method's return value; no ugly custom delegates, BeginInvoke,
EndInvoke, or IAsyncResults to deal with over and over again. It's all wrapped
up in a nice little package with one call to Invoke on the AsyncHelper class:

    
    
    void Invoke (Func call, Action callback)

Recalling some .NET 3.0+ additions, a Func<T1> represents a function that
takes no parameters and returns some value of type T1. An Action<T1>
represents a function that takes exactly one parameter of type T1 and returns
no value. These two common delegate types are a perfect fit for the pairing of
an asynchronous method call (which takes no parameters and returns a value)
with a callback (that takes exactly one parameter with no return value) to
process the results. Let's see how a simple asynchronous invocation would look
using this class:

    
    
    int x = 0;
    var async = new AsyncHelper();
    async.Invoke(
      () => 42,
      (result) => x = result * 2
    );
    async.Wait();

This sample has all the key ingredients of a successful asynchronous method
invocation. That's it! As I said, there's no dealing with BeginInvoke,
EndInvoke, IAsyncResults, or other messy things that just clutter up the
actual simplicity of the real asynchronous operation you intended to write
originally. For those of you unfamiliar with the syntax seen above, read up on
C# lambda expressions. Let's step through this sample in order to understand
what's going on:

  1. We have a local integer variable x with which to store our asynchronously-calculated results.
  2. We create a local instance of the AsyncHelper class with the default settings. If configured to do so, this will create a shared instance of Mike Woodring's custom ThreadPool and use it to queue up asynchronous operations.
  3. The Invoke() call essentially queues up in the thread pool an internal wrapper method that will execute the method represented by the first parameter (the call) and then passing its return value into the method represented by the second parameter (the callback).
  4. The asynchronous method to be executed returns the integer value 42.
  5. The callback that processes that result multiplies it by 2 and stores it in the local variable x.
  6. The Wait() call suspends the current thread's execution until all known asynchronous operations have completed. It's basically a synchronization point to make sure our results are known before we continue.

At some point in time between the Invoke() and probably during Wait()'s wait
loop, the () => 42 lambda-expression-as-a-delegate is invoked on a thread
pulled from the thread pool. It's value is returned and is passed along to the
(result) => x = result * 2 callback lambda-expression-as-a-delegate, still on
the same thread. **IDisposable** The AsyncHelper implements the IDispoable
interface. This just means that it is a candidate for usage with the using
statement, like so:

    
    
    using (var async = new AsyncHelper())
    {
      // At the end of this scope, async.Wait() is
      // called automatically.
    }

The Dispose() method call, automatically inserted by the compiler at the end
of the using block's scope, simply calls Wait() on the async instance.
Wrapping a group of asynchronous calls in a using statement is an easy way to
visually see where the thread synchonization point is instead of having to
search through your code for an explicit Wait() call. **Completion handlers**
If you have quite a few, potentially unrelated, asynchronous operations that
do not depend on each other, you can break up the logic for these operations
into separate functions and pass around the async instance. Now let's say you
want to have some post-operation logic that needs to happen after all the
asynchronous operations complete. You can do this with a Complete() handler:

    
    
    int x = 0;
    using (var async = new AsyncHelper())
    {
      async.Invoke (
        () => 42,
        (result) => x = result
      );
      async.Complete(
        () => Console.WriteLine(x)
      );
    }

The Complete() call registered a simple method to be executed AFTER all
asynchronous operations have completed. It is NOT called immediately. It is in
the Wait() method that these completion handlers are executed, in the exact
order they were registered in. This is a great pattern to use in MVC where
your controller is loading up lots of ViewData variables from various business
layer calls. The asynchronous operations can store results in some private
class fields and the completion handlers can read those private fields and set
the ViewData keys all on one single thread when all is done (since you don't
want to deal with thread-safety issues with the ViewData collection).
**Exception Handling** All exceptions thrown from asynchronous methods or
their callbacks are caught and saved until Wait() is done waiting on all
operations to complete. If any exceptions were thrown, an
AsyncOperationsFailedException exception is thrown to encapsulate the failed
operations and their corresponding exceptions. **Conditional Compilation
Constants** Two conditional compilation constants are defined in the class
file:

  * UseCustomThreadPool: Enables the use of Mike Woodring's custom ThreadPool implementation so as to not interfere with the common .NET thread pool used for other services. This constant is **enabled** by default.
  * NotAsynchronous: Completely disables all asynchronous execution and turns Invoke calls into synchronous calls. Exceptions are still wrapped, but only zero or one items will be found in the FailedOperations collection on the AsyncOperationsFailedException. Completion handlers are executed after the now-dummy Wait() call. This constant is **disabled** by default. It is intended to allow an easy degrade path back to synchronous execution without refactoring all of your code that uses AsyncHelper.

**Built-in WCF asynchronous client support** You might ask why I have gone and
created this wrapper in the first place? Why didn't I simply switch on the
"Asynchronous Support" when generating the WCF service references and use
that? The answer to that is multi-part.

  1. As proper n-tier design dictates, your business layer should perform all the service calls and data calls on behalf of your application.Let us assume you started out simple and wrote all your business layer methods to execute synchronously (i.e. returning the result of the underlying service call through the return value of the function), which is the natural path to take for any software engineer initially unconcerned with such things as asynchronicity. Your application code makes a call to a business layer method and waits for the result to come back when the necessary underlying business tasks are complete, i.e. a few data calls or a service call or two.Since your application code assumes synchronicity of the business layer, there would be no benefit to making the business layer methods invoke the service / data calls asynchronously. Furthermore, complicating that business layer interface with the typical asynchronous execution overloads per each business layer method clutters up the code both in your business layer and application code. Also, this would naturally shift the burden of maintaining the asynchronous execution state up to the application code.
  2. The default svcutil.exe and/or VS2008 generated WCF service reference client code makes use of the common .NET thread pool to process its asynchronous I/O requests. If you did your research you'd probably have found out that such long-running work items result in scalability and performance problems. To the best of my knowledge, the auto-generated service reference client code does not make use of I/O completion ports so there's effectively no difference between invoking the business layer method asynchronously and invoking the underlying service call (that the business layer method invokes) asynchronously. However, if I am wrong and the generated client code _does_ make use of I/O completion ports then perhaps a different solution is called for.
