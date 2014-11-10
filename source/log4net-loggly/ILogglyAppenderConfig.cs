namespace log4net.loggly
{
	public interface ILogglyAppenderConfig
	{
		string RootUrl { get; set; }
		string InputKey { get; set; }
		string UserAgent { get; set; }
		int TimeoutInSeconds { get; set; }
        string Tag { get; set; }
	}
}