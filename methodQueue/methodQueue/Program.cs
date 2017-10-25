using System;
using System.Collections.Generic;
using System.Threading;

namespace methodQueues_CodeSample
{
	class Program
	{
		//The singular methodQueue dictionary. Call by calling the referencing Thread.CurrentThread.
		public static Dictionary<Thread, MethodQueue> methodQueues = new Dictionary<Thread, MethodQueue>();
		private static Thread mainThread; //The Thread Main is called from.
		static void Main(string[] args)
		{
			mainThread = Thread.CurrentThread;
			//Add a methodQueue for the mainThread
			methodQueues.Add(mainThread, new MethodQueue(mainThread));

			//Start the Created Thread.
			StartThread();

			//Wait 2 seconds, then trigger check on mainThread's methodQueue clear.
			TriggerUpdate(2000);//Normally this would just be called as part of a primary loop. (IE: a Timer in WPF, or Update() in Unity)

			Console.ReadKey();
		}

		//Simple thread that just waits x milliseconds, then calls the function to clear this thread's method Queue.
		private static void TriggerUpdate(int wait)
		{
			Thread.Sleep(wait);
			methodQueues[Thread.CurrentThread].Tick();
		}

		//A simple check that determines if the thread calling this is the mainThread or the thread we creatd at runtime.
		private static void IsMainThread()
		{
			if (Thread.CurrentThread != mainThread)
			{
				Console.WriteLine("Executing From Created Thread:" + Thread.CurrentThread.ManagedThreadId);
			}
			else
			{
				Console.WriteLine("Executing From Main:" + +Thread.CurrentThread.ManagedThreadId);
			}
		}

		//Starts a new thread.
		private static void StartThread()
		{
			Thread newThread = new Thread(new ThreadStart(() => DoThreadWork()));
			newThread.IsBackground = true;
			newThread.Start();
		}

		//Runs at the start of the created thread.
		private static void DoThreadWork()
		{
			//Create a methodQueue for this thread.
			methodQueues.Add(Thread.CurrentThread, new MethodQueue(Thread.CurrentThread));

			//Setup the method to be called when passed to the mainThread.
			Action mainThreadMethod = () => IsMainThread();

			//Setup the method to be called back to this thread once execution on the main thread is complete.
			Action thisThreadCallback = () => IsMainThread();

			//Setup the remote call on the mainThread.
			methodQueues[mainThread].AddMethod(mainThreadMethod, Thread.CurrentThread, thisThreadCallback);

			//Wait 4 seconds, then check to see if the callback we defined a couple lines above is ready to be called on the created thread.
			TriggerUpdate(4000);
		}
	}
}
