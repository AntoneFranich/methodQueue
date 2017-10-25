using System;
using System.Collections.Generic;
using System.Threading;

namespace methodQueues_CodeSample
{
	public class MethodQueue
	{
		//A custom struct holding information on the delegate method to call, the thread that call came from, and the delegate callback method to be called after execution of "method" is done.
		private struct methodToRun
		{
			Action _method;//Method to be called on this methodQueue's owner thread.
			public Action method { get { return _method; } }
			Thread _source;//Thread that sent this request.
			public Thread source { get { return _source; } }
			Action _callback;//Method call to send back to _source after _method has been called.
			public Action callback { get { return _callback; } }

			public methodToRun(Action Method, Thread Source, Action Callback)
			{
				_method = Method;
				_source = Source;
				_callback = Callback;
			}
		}
		//a Queue of methods to run.
		private readonly Queue<methodToRun> _methodsToRun;
		private Thread thread;//The thread owning this queue. methods can only be executed on this thread.

		//Called when creating the MethodQueue, sets the thread and _methodToRun Queue.
		public MethodQueue(Thread _thread)
		{
			thread = _thread;
			_methodsToRun = new Queue<methodToRun>();
		}

		public void Tick()
		{
			if (Thread.CurrentThread == thread) //Checks to ensure that we can only be run on the Thread which owns this MethodQueue
			{
				lock (_methodsToRun) //Lock the methodQueue to ensure thread safety.
				{
					while (_methodsToRun.Count > 0) //Run through all queued methods.
					{
						methodToRun thisMethod = _methodsToRun.Dequeue(); //grab this method information.
						thisMethod.method.Invoke(); //Run the method.
						if (thisMethod.source != null && thisMethod.callback != null) //If information for a callback is present, trigger the setup so that it's added to the source thread's MethodQueue.
							setupCallBack(thisMethod);
					}
				}
			}
		}

		//Quick call to setup a the callback method on the _source Thread's methodQueue.
		private void setupCallBack(methodToRun thisMethod)
		{
			Program.methodQueues[thisMethod.source].AddMethod(thisMethod.callback);
		}

		//Add a method with no callback information.
		public void AddMethod(Action action)
		{
			AddMethod(action, null, null);
		}

		//Add a method with callback information.
		public void AddMethod(Action action, Thread caller, Action callback)
		{
			lock (_methodsToRun) //Always lock the _methodsToRun Queue to ensure thread safety.
			{
				_methodsToRun.Enqueue(new methodToRun(action, caller, callback));
			}
		}
	}
}
