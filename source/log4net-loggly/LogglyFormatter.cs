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
            //NOTE: Empty objects are sent as string.Empty because empty objects are not indexed in the Loggly
            //so they won't show up in the json messages

            //managing exceptions
            object exceptionInfo = getExceptionInfo(loggingEvent);
            
            //managing messages
            string message = getMessageInfo(loggingEvent);
            
            //managing custom objects
            object objInfo = getCustomObjectInfo(loggingEvent);

			return new
            {
                level = loggingEvent.Level.DisplayName,
                timeStamp = loggingEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
                hostName = Environment.MachineName,
                process = _currentProcess.ProcessName,
                threadName = loggingEvent.ThreadName,
                message = message,
                objectInfo = objInfo,
                exceptionObject = exceptionInfo,
                loggerName=loggingEvent.LoggerName
            };
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
            object exceptionInfo = string.Empty;
            if (loggingEvent.ExceptionObject != null)
            {
                object innerException = string.Empty;

                //most of the times .net exceptions contain important messages in the inner exceptions
                if (loggingEvent.ExceptionObject.InnerException != null)
                {
                    innerException = new
                    {
                        innerExceptionType = loggingEvent.ExceptionObject.InnerException.GetType().FullName,
                        innerExceptionMessage = loggingEvent.ExceptionObject.InnerException.Message ?? string.Empty,
                        innerStacktrace = loggingEvent.ExceptionObject.InnerException.StackTrace ?? string.Empty,
                    };
                }

                exceptionInfo = new
                {
                    exceptionType = loggingEvent.ExceptionObject.GetType().FullName,
                    exceptionMessage = loggingEvent.ExceptionObject.Message ?? string.Empty,
                    stacktrace = loggingEvent.ExceptionObject.StackTrace ?? string.Empty,
                    innerExceptionInfo = innerException
                };
            }
            return exceptionInfo;
        }

        /// <summary>
        /// Returns a string type message if it is not a custom object
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private string getMessageInfo(LoggingEvent loggingEvent)
        {
            string message = string.Empty;
            if (loggingEvent.MessageObject.GetType() == typeof(string))
            {
                message = loggingEvent.MessageObject.ToString();
            }
            //if it is sent by using InfoFormat method then treat it as a string message
            else if (loggingEvent.MessageObject.GetType().FullName == "log4net.Util.SystemStringFormat")
            {
                message = loggingEvent.MessageObject.ToString();
            }

            return message;
        }

        /// <summary>
        /// Returns the custom object info if it is not a string. Else string.Empty
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private object getCustomObjectInfo(LoggingEvent loggingEvent)
        {
            //Loggly does not allow to same field vary it types
            //like "message" field can't be used for both string type message and other custom objects
            
            object objInfo = string.Empty;
            
            if(loggingEvent.MessageObject.GetType() != typeof(string)
                && loggingEvent.MessageObject.GetType().FullName != "log4net.Util.SystemStringFormat")
            {
                objInfo = loggingEvent.MessageObject;
            }

            return objInfo;
        }
	}
}
