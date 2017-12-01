namespace log4net.loggly
{
	public class LogglyAppenderConfig : ILogglyAppenderConfig
	{
		private string _rootUrl;
		private string _logMode;
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
			}
		}

		public string LogMode
		{
			get { return _logMode; }
			set
			{
				_logMode = value;
				if (!_logMode.EndsWith("/"))
				{
					_logMode = _logMode.ToLower() + "/";
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
			LogMode = "bulk";
			LogicalThreadContextKeys = null;
			GlobalContextKeys = null;
		}
	}
}