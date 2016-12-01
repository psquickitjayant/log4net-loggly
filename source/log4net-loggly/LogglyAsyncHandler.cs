using System.Collections.Concurrent;
using System.Threading;

namespace log4net.loggly
{
	class LogglyAsyncHandler
	{
		// Size of the internal event queue. 
		//protected const int QueueSize = 32768;
		protected ILogglyAppenderConfig _config;

		protected bool IsRunning = false;
		//static list of all the queues the le appender might be managing.
		private static ConcurrentBag<BlockingCollection<string>> _allQueues = new ConcurrentBag<BlockingCollection<string>>();

		public static ILogglyClient Client = new LogglyClient();
		public LogglyAsyncHandler()
		{
			Queue = new BlockingCollection<string>();
			_allQueues.Add(Queue);
			WorkerThread = new Thread(new ThreadStart(SendLogs));
			WorkerThread.Name = "Loggly Log Appender";
			WorkerThread.IsBackground = true;
		}

		protected readonly BlockingCollection<string> Queue;
		protected Thread WorkerThread;

		protected virtual void SendLogs()
		{
			if (this._config != null)
			{
				while (true)
				{
					var msg = Queue.Take();
					Client.Send(this._config, msg);
				}
			}
		}

		public void PostMessage(string msg, ILogglyAppenderConfig config)
		{
			this._config = config;
			if (!IsRunning)
			{
				WorkerThread.Start();
				IsRunning = true;
			}
			if (!Queue.TryAdd(msg))
			{
				Queue.Take();
				Queue.TryAdd(msg);
			}
		}
	}

}