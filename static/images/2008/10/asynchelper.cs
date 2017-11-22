// AsyncHelper utility class.
// James Dunne

// 10/10/2008   - Initial version.
// 10/13/2008   - Included support for Mike Woodring's custom ThreadPool implementation.
//              - Included exception handling support.
//              - Added NotAsynchronous conditional compilation switch.

// -------------------------------------------------------------------------------------
// -------------------------- CUSTOM THREAD POOL SWITCH --------------------------------
// -------------------------------------------------------------------------------------

// Switch this on to use Mike Woodring's custom ThreadPool implementation instead of the
// default .NET ThreadPool.  The .NET ThreadPool warns that it should not be used for
// "long-running" tasks and could interfere with core services such as ASP.NET and WCF
// scheduling.

#define UseCustomThreadPool

// -------------------------------------------------------------------------------------
// --------------------------- SYNCHRONOUS MODE SWITCH ---------------------------------
// -------------------------------------------------------------------------------------

// Switch this flag on to bypass all asynchronous behavior in case of serious issues or
// performance problems discovered during tests.  In this mode all Invoke calls will
// perform synchronously on the same calling thread and all "Complete" handlers will be
// invoked when Wait()/Dispose() is executed, consistent with the asynchronous control
// flow.  Exceptions will be caught during the Invoke calls and will be wrapped in
// the custom AsyncOperationsFailedException type.  The custom exception will only hold
// the one exception encountered in the FailedOperations collection and is essentially
// a compatibility wrapper in this mode.

//#define NotAsynchronous

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace WellDunne.Utility.AsyncOperations
{
    #region Supporting interface

    /// <summary>
    /// Public interface to base the two common types of asynchronous operation state holders.
    /// </summary>
    public interface IAsyncOperationState
    {
        ManualResetEvent OperationCompleted { get; set; }
        Exception ThrownException { get; set; }
    }

    #endregion

    #region AsyncOperationsFailedException exception

    /// <summary>
    /// An exception thrown by Wait() or Dispose() on AsyncHelper if one or more exceptions
    /// occurred during execution of any of the asynchronous operations.
    /// </summary>
    public class AsyncOperationsFailedException : Exception
    {
        public AsyncOperationsFailedException(IAsyncOperationState[] failedOperations)
        {
            this.FailedOperations = failedOperations;
        }

        public IAsyncOperationState[] FailedOperations { get; private set; }

        public override string Message
        {
            get
            {
#if !NotAsynchronous
                return String.Format(
                    "One or more asynchronous operations failed. Please check each ThrownException property within the FailedOperations collection of this exception for more details.{1}{0}",
                    String.Join(Environment.NewLine, (from e in FailedOperations select e.ThrownException.Message).ToArray()),
                    Environment.NewLine
                );
#else
                // Synchronous mode:
                return String.Format(
                    "[SYNCHRONOUS MODE]: This exception type is now just a wrapper around the thrown exception " +
                    "from the asynchronous operation executed synchronously because the conditional compilation " +
                    "constant 'NotAsynchronous' was enabled in AsyncHelper.cs.  Original exception detail follows:{1}{0}",
                    String.Join(Environment.NewLine, (from e in FailedOperations select e.ThrownException.Message).ToArray()),
                    Environment.NewLine
                );
#endif
            }
        }

        public override string ToString()
        {
#if !NotAsynchronous
            return String.Format(
                "One or more asynchronous operations failed.{1}{0}",
                String.Join(Environment.NewLine, (from e in FailedOperations select e.ThrownException.ToString()).ToArray()),
                Environment.NewLine
            );
#else
            // Synchronous mode:
            return String.Format(
                "[SYNCHRONOUS MODE]: This exception type is now just a wrapper around the thrown exception " +
                "from the asynchronous operation executed synchronously because the conditional compilation " +
                "constant 'NotAsynchronous' was enabled in AsyncHelper.cs.  Original exception detail follows:{1}{0}",
                String.Join(Environment.NewLine, (from e in FailedOperations select e.ThrownException.Message).ToArray()),
                Environment.NewLine
            );
#endif
        }
    }

    #endregion

    #region AsyncHelper implementation

    /// <summary>
    /// A helper class to wrap up multiple asynchronous calls to long-running business
    /// layer methods without the programmer needing to worry about IAsyncResult,
    /// BeginInvoke, EndInvoke and other synchronization primitives.
    /// </summary>
    /// <remarks>
    /// <para>Wrap an instance of this class in a using statement.  When the using scope is
    /// about to be exited, the instance will wait for all asynchronous operations to
    /// be completed then it will call all the registered Complete handlers in the order
    /// they were registered.</para>
    /// <para>The .NET ThreadPool is used to handle the asynchronous calls.  Use
    /// ThreadPool.SetMinThreads to ensure that at least N threads will be used.</para>
    /// <para>Just like the MulticastDelegate, exceptions MUST be caught while executing
    /// the business layer method, otherwise undefined behavior occurs and you cannot
    /// rely on the collective final state of your callback methods.</para>
    /// </remarks>
    public class AsyncHelper : IDisposable
    {
        #region Private state

#if !NotAsynchronous
        /// <summary>
        /// A list of handles (ManualResetEvents) to wait on.
        /// </summary>
        private List<IAsyncOperationState> _invocations = new List<IAsyncOperationState>();
#endif
        
        /// <summary>
        /// A list of methods to execute when all operations are complete.
        /// </summary>
        private List<Action> _completes = new List<Action>();

#if !NotAsynchronous
#  if UseCustomThreadPool
        private const int InitialThreadCount = 10;
        private const int MaxThreadCount = 10;

        /// <summary>
        /// A custom ThreadPool implementation that does not conflict with the
        /// standard .NET thread pool.  This one is used for "long-running" tasks
        /// like I/O operations and service calls.
        /// </summary>
        private static ThreadPool _defaultPool;

        private ThreadPool _pool;
#  endif
#endif

        #endregion

        #region Constructors

#if !NotAsynchronous
#  if UseCustomThreadPool
        public AsyncHelper()
        {
            if (_defaultPool == null)
                _defaultPool = new ThreadPool(InitialThreadCount, MaxThreadCount, "AsyncHelper");
            _pool = _defaultPool;
            if (!_pool.IsStarted)
                _pool.Start();
        }

        public AsyncHelper(ThreadPool threadPool)
        {
            _pool = threadPool;
            if (!_pool.IsStarted)
                _pool.Start();
        }
#  else
        public AsyncHelper()
        {
        }
#  endif
#else
        public AsyncHelper()
        {
        }
#endif

        #endregion

        #region Public methods

        /// <summary>
        /// Suspends the current thread until all currently running and already-completed
        /// asynchronous calls complete or throw an exception. The registered Complete
        /// actions will be called in the order they were registered if and only if no
        /// exceptions were thrown while processing all asynchronous operations.
        /// </summary>
        [DebuggerNonUserCode()]
        public void Wait()
        {
#if !NotAsynchronous
            try
            {
                // Wait on all WaitHandles:
                WaitHandle.WaitAll(
                    (from inv in _invocations
                     select (WaitHandle)inv.OperationCompleted).ToArray()
                );
            }
            finally
            {
                // Release all WaitHandle resources:
                _invocations.ForEach(op => { op.OperationCompleted.Close(); op.OperationCompleted = null; });
            }

            // Check if any exceptions were raised during asynchronous execution:
            IAsyncOperationState[] failedOperations = (
                from inv in _invocations
                where inv.ThrownException != null
                select inv).ToArray();

            // Throw a custom wrapper exception to summarize the exceptions encountered:
            if (failedOperations.Length > 0)
                throw new AsyncOperationsFailedException(failedOperations);
#endif

#if !NotAsynchronous
            // Clear out and refresh current state:
            _invocations = null;
            _invocations = new List<IAsyncOperationState>();
#endif

            // Call all the complete methods:
            _completes.ForEach(c => c());

            _completes = null;
            _completes = new List<Action>();
        }

        /// <summary>
        /// Registers an action to be performed when all asynchronous operations are completed.
        /// No Complete actions will be executed if one or more exceptions are thrown by the
        /// asynchronous operations.
        /// </summary>
        /// <param name="call">An action to perform when all asynchronous operations are completed.</param>
        public void Complete(Action call)
        {
            if (call == null)
                throw new ArgumentNullException("call");

            _completes.Add(call);
        }

        #endregion

        #region Invoke with return value

        /// <summary>
        /// Invokes a business layer method asynchronously which returns a <typeparamref name="TResult"/>
        /// and then calls <paramref name="callback"/> on that return value to perform some operation
        /// on the result.
        /// </summary>
        /// <typeparam name="TResult">Type of objects that the business layer method returns</typeparam>
        /// <param name="call">A delegate to call a business layer method and capture its return value that is of type <typeparamref name="TResult"/></param>
        /// <param name="callback">A callback function to perform some operation on the result of the business layer call.</param>
        public void Invoke<TResult>(Func<TResult> call, Action<TResult> callback)
        {
            if (call == null)
                throw new ArgumentNullException("call");
            if (callback == null)
                throw new ArgumentNullException("callback");

#if !NotAsynchronous

            // Create a state object to keep track of the call:
            AsyncOperationState<TResult> state = new AsyncOperationState<TResult>(call, callback);

            // This must be created before beginning invocation so the FireCallAndCallback
            // callback will have a valid instance to reset.
            state.OperationCompleted = new ManualResetEvent(false);

            bool isRequested = true;

#  if UseCustomThreadPool
            // Request the call with the custom ThreadPool:
            isRequested = _pool.PostRequest(new WorkRequestDelegate(FireCallAndCallback<TResult>), state);
#  else
            // Request the call with the standard ThreadPool:
            IAsyncResult result = call.BeginInvoke(new AsyncCallback(CallCompleted<TResult>), state);
#  endif

            if (!isRequested)
            {
                // Release the WaitHandle resources:
                state.OperationCompleted.Close();
                return;
            }

            // Register the WaitHandle so Wait() knows what to wait on:
            _invocations.Add(state);
#else
            // Synchronous mode:
            try
            {
                TResult val = call();
                callback(val);
            }
            catch (Exception ex)
            {
                throw new AsyncOperationsFailedException(
                    new IAsyncOperationState[] {
                        new AsyncOperationState<TResult>(call, callback) {
                            ThrownException = ex
                        }
                    }
                );
            }
#endif
        }

#if !NotAsynchronous
#  if UseCustomThreadPool
        private static void FireCallAndCallback<TResult>(object stateObj, DateTime requestEnqueueTime)
        {
            AsyncOperationState<TResult> state = (AsyncOperationState<TResult>)stateObj;

            try
            {
                // Invoke the call:
                TResult val = state.Call();
                // Invoke the callback with the result:
                state.Callback(val);
            }
            catch (Exception ex)
            {
                // Keep track of the exception and pass it back:
                state.ThrownException = ex;
            }

            // Signal to the Wait() method that this asynchronous operation is complete:
            state.OperationCompleted.Set();
        }
#  else
        /// <summary>
        /// Private handler that is invoked when the asynchronous call completed.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        private static void CallCompleted<TResult>(IAsyncResult result)
        {
            if (!result.IsCompleted)
                return;

            // Get the state we associated with this call:
            AsyncOperationState<TResult> state = (AsyncOperationState<TResult>)result.AsyncState;

            try
            {
                // EndInvoke on the call to get the exception or return value:
                TResult returnValue = state.Call.EndInvoke(result);

                // Call the callback function on the return value:
                state.Callback(returnValue);
            }
            catch (Exception ex)
            {
                // Keep track of the exception and pass it back:
                state.ThrownException = ex;
            }

            // Signal to the Wait() method that this asynchronous operation is complete:
            state.OperationCompleted.Set();
        }
#  endif
#endif

        #endregion

        #region Invoke with no return value

        /// <summary>
        /// Invokes a business layer method asynchronously which has no return value.
        /// </summary>
        /// <param name="call"></param>
        public void Invoke(Action call)
        {
            if (call == null)
                throw new ArgumentNullException("call");

#if !NotAsynchronous
            // Create a state object to keep track of the call:
            AsyncOperationStateNoResult state = new AsyncOperationStateNoResult(call);

            // This must be created before beginning invocation so the FireCallAndCallback
            // callback will have a valid instance to reset.
            state.OperationCompleted = new ManualResetEvent(false);

            bool isRequested = true;

#  if UseCustomThreadPool
            // Request the call with the custom ThreadPool:
            isRequested = _pool.PostRequest(new WorkRequestDelegate(FireCallNoCallback), state);
#  else
            // Request the call with the standard ThreadPool:
            IAsyncResult result = call.BeginInvoke(new AsyncCallback(CallCompletedNoResult), state);
#  endif

            if (!isRequested)
            {
                // Release the WaitHandle resources:
                state.OperationCompleted.Close();
                return;
            }

            // Register the WaitHandle so Wait() knows what to wait on:
            _invocations.Add(state);
#else
            // Synchronous mode:
            try
            {
                call();
            }
            catch (Exception ex)
            {
                throw new AsyncOperationsFailedException(
                    new IAsyncOperationState[] {
                        new AsyncOperationStateNoResult(call) {
                            ThrownException = ex
                        }
                    }
                );
            }
#endif
        }

#if !NotAsynchronous
#  if UseCustomThreadPool
        private static void FireCallNoCallback(object stateObj, DateTime requestEnqueueTime)
        {
            AsyncOperationStateNoResult state = (AsyncOperationStateNoResult)stateObj;

            try
            {
                // Invoke the call:
                state.Call();
            }
            catch (Exception ex)
            {
                state.ThrownException = ex;
            }

            // Signal to the Wait() method that this asynchronous operation is complete:
            state.OperationCompleted.Set();
        }
#  else
        private static void CallCompletedNoResult(IAsyncResult stateObj)
        {
            // Get the state we associated with this call:
            AsyncOperationStateNoResult state = (AsyncOperationStateNoResult)stateObj.AsyncState;

            try
            {
                // EndInvoke the delegate to get the exception or successful result:
                state.Call.EndInvoke(stateObj);
            }
            catch (Exception ex)
            {
                // Keep track of the exception and pass it back:
                state.ThrownException = ex;
            }

            // Signal to the Wait() method that this asynchronous operation is complete:
            state.OperationCompleted.Set();
        }
#  endif
#endif

        #endregion

        #region IDisposable Members

        /// <summary>
        /// When disposing, i.e. when the scope of the using statement is about to be exited,
        /// call the Wait() method.
        /// </summary>
        [DebuggerNonUserCode()]
        public void Dispose()
        {
            Wait();
        }

        #endregion

        #region AsyncOperationState class

        private class AsyncOperationState<TResult> : IAsyncOperationState
        {
            public readonly Func<TResult> Call;
            public readonly Action<TResult> Callback;

            public AsyncOperationState(Func<TResult> call, Action<TResult> callback)
            {
                this.Call = call;
                this.Callback = callback;
            }

            #region IAsyncOperationState Members

            public ManualResetEvent OperationCompleted
            {
                get;
                set;
            }

            public Exception ThrownException
            {
                get;
                set;
            }

            #endregion
        }

        #endregion

        #region AsyncOperationStateNoResult class

        private class AsyncOperationStateNoResult : IAsyncOperationState
        {
            public readonly Action Call;

            public AsyncOperationStateNoResult(Action call)
            {
                this.Call = call;
            }

            #region IAsyncOperationState Members

            public ManualResetEvent OperationCompleted
            {
                get;
                set;
            }

            public Exception ThrownException
            {
                get;
                set;
            }

            #endregion
        }

        #endregion
    }

    #endregion

    #region Test Harness

    /// <summary>
    /// A simple test harness for the AsyncHelper to demonstrate a working
    /// example.
    /// </summary>
    public static class AsyncHelperTester
    {
        public static void Test()
        {
#if !UseCustomThreadPool
            // SetMinThreads is useful here.  If this is not called,
            // a minimum of 2 threads will be used and the execution
            // time will be drastically increased, i.e. about 500msec
            // for ten Thread.Sleep(100)s.
            System.Threading.ThreadPool.SetMinThreads(10, 10);
            System.Threading.ThreadPool.SetMaxThreads(20, 20);
#endif

            // Create an x[] array to hold the results of all 10 calls:
            int[] x = new int[10];

            // Start a timer to test our asynchronicity (word?):
            long start = DateTime.Now.Ticks;

            // Create the AsyncHelper to handle multiple asynchronous invocations of
            // business layer methods.
            using (var async = new AsyncHelper())
            {
                Console.WriteLine("Beginning call...");

                // Test a business layer call with no return value:
                async.Invoke(() => { Thread.Sleep(600); Console.WriteLine("Sleeper!"); });

                // Fire off 10 asynchronous "business layer" method calls:
                for (int i = 0; i < 10; ++i)
                {
                    // Have to copy the iterator variable locally so it can be
                    // passed in to the closures created by the anonymous
                    // delegates.  Normally you wouldn't have to do this writing
                    // normal business layer calls... unless you're trying to do
                    // something clever.
                    int v = i;

                    // Create an asynchronous call:
                    // The <int> is to specify that the expected return type of the
                    // business layer method is of type 'int'.
                    async.Invoke<int>(
                        // The business layer caller:
                        () =>
                        {
                            // Our "business layer" method:
                            Thread.Sleep(100);
                            // Just return the temp counter variable (0-9):
                            return v;
                        },

                        // The callback function to handle the result of the business layer call:
                        (result) =>
                        {
                            // Our call-back to process the business layer result:
                            Console.WriteLine("Thread[{0}]: result = {1}", Thread.CurrentThread.ManagedThreadId, result);
                            // Store the result into the x[] array:
                            x[v] = result + 10;
                        }
                    );
                }

                // Register a Complete handler that gets called when all async operations complete.
                async.Complete(() =>
                {
                    // Write out the values of the x[] array:
                    Console.WriteLine("Complete():");
                    Console.WriteLine("x = [ {0} ]", String.Join(", ", (from v in x select v.ToString()).ToArray()));
                });

                // At the end of this scope, Dispose() is called on the async object
                // which in turn calls the Wait() method that waits on all the above
                // created asynchronous calls and callbacks to complete.
                Console.WriteLine("Waiting...");
            }

            // When we get here, all the calls should be complete and the x[] array
            // should be fully populated.

            // If all goes well, the total execution time should be just over 100msec,
            // and in my tests I get 109.375msec, which is excellent. Basically, we're
            // firing off 10 separate calls that each wait for 100msec and since they're
            // all executing at the same time they're all waiting for essentially the same
            // 100msec time span. The 9.375msec is expected minor overhead for thread
            // context switching and other logic, perhaps garbage collections and other
            // things not apparently obvious in the code.

            long end = DateTime.Now.Ticks;
            Console.WriteLine("Took {0} milliseconds.", new TimeSpan(end - start).TotalMilliseconds);
        }

        /// <summary>
        /// Sneaks in an exception on the last business layer call...
        /// </summary>
        public static void TestFailure()
        {
#if !UseCustomThreadPool
            // SetMinThreads is useful here.  If this is not called,
            // a minimum of 2 threads will be used and the execution
            // time will be drastically increased, i.e. about 500msec
            // for ten Thread.Sleep(100)s.
            System.Threading.ThreadPool.SetMinThreads(10, 10);
            System.Threading.ThreadPool.SetMaxThreads(20, 20);
#endif

            // Create an x[] array to hold the results of all 10 calls:
            int[] x = new int[10];

            // Start a timer to test our asynchronicity (word?):
            long start = DateTime.Now.Ticks;

            try
            {
                // Create the AsyncHelper to handle multiple asynchronous invocations of
                // business layer methods.
                using (var async = new AsyncHelper())
                {
                    Console.WriteLine("Beginning 10 calls...");

                    // Test a business layer call with no return value:
                    async.Invoke(() => { Thread.Sleep(600); Console.WriteLine("Sleeper!"); });

                    // Test a business layer call with no return value throwing an exception:
                    async.Invoke(() => { Thread.Sleep(300); throw new Exception("Sleeper FAIL!"); });

                    // Fire off 10 asynchronous "business layer" method calls:
                    for (int i = 0; i < 10; ++i)
                    {
                        // Have to copy the iterator variable locally so it can be
                        // passed in to the closures created by the anonymous
                        // delegates.  Normally you wouldn't have to do this writing
                        // normal business layer calls... unless you're trying to do
                        // something clever.
                        int v = i;

                        // Create an asynchronous call:
                        // The <int> is to specify that the expected return type of the
                        // business layer method is of type 'int'.
                        async.Invoke<int>(
                            // The business layer caller:
                            () =>
                            {
                                // Our "business layer" method:
                                Thread.Sleep(100);

                                // Test an exception:
                                if (v == 9)
                                    throw new Exception("Failed!");

                                // Just return the temp counter variable (0-9):
                                return v;
                            },

                            // The callback function to handle the result of the business layer call:
                            (result) =>
                            {
                                // Our call-back to process the business layer result:
                                Console.WriteLine("Thread[{0}]: result = {1}", Thread.CurrentThread.ManagedThreadId, result);
                                // Store the result into the x[] array:
                                x[v] = result + 10;
                            }
                        );
                    }

                    // Register a Complete handler that gets called when all async operations complete.
                    async.Complete(() =>
                    {
                        // Write out the values of the x[] array:
                        Console.WriteLine("Complete():");
                        Console.WriteLine("x = [ {0} ]", String.Join(", ", (from v in x select v.ToString()).ToArray()));
                    });

                    // At the end of this scope, Dispose() is called on the async object
                    // which in turn calls the Wait() method that waits on all the above
                    // created asynchronous calls and callbacks to complete.
                    Console.WriteLine("Waiting for completion...");
                }
            }
            catch (AsyncOperationsFailedException ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // When we get here, all the calls should be complete and the x[] array
            // should be fully populated.

            // If all goes well, the total execution time should be just over 100msec,
            // and in my tests I get 109.375msec, which is excellent. Basically, we're
            // firing off 10 separate calls that each wait for 100msec and since they're
            // all executing at the same time they're all waiting for essentially the same
            // 100msec time span. The 9.375msec is expected minor overhead for thread
            // context switching and other logic, perhaps garbage collections and other
            // things not apparently obvious in the code.

            long end = DateTime.Now.Ticks;
            Console.WriteLine("Took {0} milliseconds.", new TimeSpan(end - start).TotalMilliseconds);
        }

        public static void LoadTest()
        {
            // The result of this program should print '258' in all circumstances.

#if !NotAsynchronous
#  if UseCustomThreadPool
            Debug.WriteLine("Creating and starting custom ThreadPool.");
            ThreadPool pool = new ThreadPool(6, 6, "AsyncHelper custom");
            pool.Start();

            var async = new AsyncHelper(pool);
#  else
            var async = new AsyncHelper();
#  endif
#else
            var async = new AsyncHelper();
#endif

            Debug.WriteLine("Sleep 100ms");
            Thread.Sleep(100);

            long x;
            long start, end;

            for (int i = 0; i < 4; ++i)
            {
                x = i + 1;

                Debug.WriteLine("Start timer");

                start = DateTime.Now.Ticks;
                async.Invoke(() => { Thread.Sleep(1000); x *= 2; });
                async.Invoke(() => { Thread.Sleep(1000); x *= 2; });
                async.Invoke(() => { Thread.Sleep(1000); x *= 2; });
                // This will be executed after Wait() is done waiting for all operations to complete.
                async.Complete(() => Debug.WriteLine(x));
                async.Invoke(() => { Thread.Sleep(1000); x *= 2; });
                async.Invoke(() => { Thread.Sleep(1000); x *= 2; });
                async.Invoke(() => { Thread.Sleep(1000); x *= 2; });
                async.Wait();
                end = DateTime.Now.Ticks;

                Debug.WriteLine(String.Format("End timer: {0} ms", new TimeSpan(end - start).TotalMilliseconds));
            }
        }
    }

    #endregion
}
