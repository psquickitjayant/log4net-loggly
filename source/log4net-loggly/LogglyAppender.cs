using log4net.Appender;
using log4net.Core;
using System.Threading;
using System.Threading.Tasks;


namespace log4net.loggly
{
	public class LogglyAppender : AppenderSkeleton
	{
		public static readonly string InputKeyProperty = "LogglyInputKey";

		public static ILogglyFormatter Formatter = new LogglyFormatter();
		public static ILogglyClient Client = new LogglyClient();

		private ILogglyAppenderConfig Config = new LogglyAppenderConfig();

		public string RootUrl { set { Config.RootUrl = value; } }
		public string InputKey { set { Config.InputKey = value; } }
		public string UserAgent { set { Config.UserAgent = value; } }
		public int TimeoutInSeconds { set { Config.TimeoutInSeconds = value; } }
        public string Tag { set { Config.Tag = value; } }

        protected override void Append(LoggingEvent loggingEvent)
        {
            loggingEvent.Properties["LoggingThread"] = Thread.CurrentThread.Name;
            ThreadPool.QueueUserWorkItem(x => SendLogAction(loggingEvent));
        }

        private void SendLogAction(LoggingEvent loggingEvent)
        {
            Formatter.AppendAdditionalLoggingInformation(Config, loggingEvent);
            Client.Send(Config, Formatter.ToJson(loggingEvent));
        }

	}
}
