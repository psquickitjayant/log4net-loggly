using System.Collections.Generic;
using log4net.Core;
using System;

namespace log4net.loggly
{
	public interface ILogglyFormatter
	{
		void AppendAdditionalLoggingInformation(ILogglyAppenderConfig unknown, LoggingEvent loggingEvent);
		string ToJson(LoggingEvent loggingEvent);
		string ToJson(IEnumerable<LoggingEvent> loggingEvents);

        /// <summary>
        /// Merged Layout formatted log with the formatted timestamp
        /// </summary>
        /// <param name="renderedLog"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        string ToJson(string renderedLog, DateTime timeStamp);
        
	}
}