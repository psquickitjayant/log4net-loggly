namespace log4net.loggly
{
	public class LogglyAppenderConfig: ILogglyAppenderConfig
	{
		private string _rootUrl;
		public string RootUrl
		{
			get { return _rootUrl; }
			set
			{
				//TODO: validate http and uri
				_rootUrl = value;
				if (!_rootUrl.EndsWith("/"))
				{
					_rootUrl += "/";
				}
				if (!_rootUrl.EndsWith("inputs/"))
				{
					_rootUrl += "inputs/";
				}
			}
		}

		public string InputKey { get; set; }

		public string UserAgent { get; set; }

		public int TimeoutInSeconds { get; set; }

        public string Tag { get; set; }

        public string LogicalThreadContextKeys { get; set; }

        public string GlobalContextKeys { get; set; }

        public LogglyAppenderConfig()
		{
			UserAgent = "loggly-log4net-appender";
			TimeoutInSeconds = 30;
            Tag = "log4net";
            LogicalThreadContextKeys = null;
            GlobalContextKeys = null;
		}
	}
}