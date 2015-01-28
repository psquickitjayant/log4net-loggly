using log4net.Appender;
using log4net.Core;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


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
            SendLogAction(loggingEvent);
        }

        private void SendLogAction(LoggingEvent loggingEvent)
        {
            Formatter.AppendAdditionalLoggingInformation(Config, loggingEvent);

            //we should always format event in the same thread as 
            //many properties used in the event are associated with the current thread
            //like threadname, ndc stacks, threadcontent properties etc.
            string _formattedLog = Formatter.ToJson(loggingEvent);
            ThreadPool.QueueUserWorkItem(x => Client.Send(Config, _formattedLog));
        }

	}
}
