namespace log4net.loggly
{
	public interface ILogglyAppenderConfig
	{
		string RootUrl { get; set; }
		string InputKey { get; set; }
		string UserAgent { get; set; }
		string LogMode { get; set; }
		int TimeoutInSeconds { get; set; }
		string Tag { get; set; }
		string LogicalThreadContextKeys { get; set; }
		string GlobalContextKeys { get; set; }
	}
}