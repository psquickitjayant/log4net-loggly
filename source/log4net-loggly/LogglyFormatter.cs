using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net.Core;
using Newtonsoft.Json;

namespace log4net.loggly
{
	public class LogglyFormatter : ILogglyFormatter
	{
		private Process _currentProcess;

		public LogglyFormatter()
		{
			_currentProcess = Process.GetCurrentProcess();
		}

		public virtual void AppendAdditionalLoggingInformation(ILogglyAppenderConfig config, LoggingEvent loggingEvent)
		{
		}

	    public virtual string ToJson(LoggingEvent loggingEvent)
	    {
            return JsonConvert.SerializeObject(PreParse(loggingEvent)); 
	    }

        public virtual string ToJson(IEnumerable<LoggingEvent> loggingEvents)
		{
            return JsonConvert.SerializeObject(loggingEvents.Select(PreParse));
		}

        private object PreParse(LoggingEvent loggingEvent)
		{
            //NOTE: 
            //1. Empty objects are sent as string.Empty because empty objects are not indexed in the Loggly so they won't show up in the json messages
            //2. null fields are shown as "none" in the Loggly

            //managing exceptions
            object _exceptionInfo = getExceptionInfo(loggingEvent);
            
            //managing messages and custom objects
            object _objInfo = string.Empty;
            string _message = getMessageAndObjectInfo(loggingEvent, out _objInfo);
            
            dynamic _loggingInfo = new
            {
                timestamp = loggingEvent.TimeStamp.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffzzz"),
                level = loggingEvent.Level.DisplayName,
                hostName = Environment.MachineName,
                process = _currentProcess.ProcessName,
                threadName = loggingEvent.Properties["LoggingThread"] ?? loggingEvent.ThreadName,
                message = _message,
                objectInfo = _objInfo,
                exceptionObject = _exceptionInfo,
                loggerName=loggingEvent.LoggerName
            };

            return _loggingInfo;
		}

        /// <summary>
        /// Returns the exception information. Also takes care of the InnerException.  
        /// If it is null then returns string.Empty
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private object getExceptionInfo(LoggingEvent loggingEvent)
        {
            //managing exceptions
            dynamic exceptionInfo = string.Empty;
            if (loggingEvent.ExceptionObject != null)
            {
                //if there is not any inner exception, then it must be shown as "None" in the loggly 
                dynamic innerException = null;

                //most of the times .net exceptions contain important messages in the inner exceptions
                if (loggingEvent.ExceptionObject.InnerException != null)
                {
                    innerException = new
                    {
                        innerExceptionType = loggingEvent.ExceptionObject.InnerException.GetType().FullName,
                        innerExceptionMessage = loggingEvent.ExceptionObject.InnerException.Message,
                        innerStacktrace = loggingEvent.ExceptionObject.InnerException.StackTrace,
                    };
                }

                exceptionInfo = new
                {
                    exceptionType = loggingEvent.ExceptionObject.GetType().FullName,
                    exceptionMessage = loggingEvent.ExceptionObject.Message,
                    stacktrace = loggingEvent.ExceptionObject.StackTrace,
                    innerExceptionInfo = innerException
                };
            }
            return exceptionInfo;
        }

        /// <summary>
        /// Returns a string type message if it is not a custom object,
        /// otherwise returns custom object details
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private string getMessageAndObjectInfo(LoggingEvent loggingEvent, out object objInfo)
        {
            string message = string.Empty;
            
            if (loggingEvent.MessageObject.GetType() == typeof(string)
                //if it is sent by using InfoFormat method then treat it as a string message
                || loggingEvent.MessageObject.GetType().FullName == "log4net.Util.SystemStringFormat"
                || loggingEvent.MessageObject.GetType().FullName.Contains("StringFormatFormattedMessage"))
            {
                message = loggingEvent.MessageObject.ToString();
                objInfo = string.Empty;
            }
            else
            {
                objInfo = loggingEvent.MessageObject;
            }
            return message;
        }
    }
}
