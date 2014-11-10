namespace log4net.loggly
{
	public interface ILogglyClient
	{
		void Send(ILogglyAppenderConfig config, string inputKey, string userAgent, string tag, string message);
	}
}