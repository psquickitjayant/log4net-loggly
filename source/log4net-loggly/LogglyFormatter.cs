using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net.Core;
using Newtonsoft.Json;
using System.Dynamic;

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

        /// <summary>
        /// Formats the log event to various JSON fields that are to be shown in Loggly.
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private object PreParse(LoggingEvent loggingEvent)
		{
            //formating base logging info
            dynamic _loggingInfo = new ExpandoObject();
            _loggingInfo.timestamp = loggingEvent.TimeStamp.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffzzz");
            _loggingInfo.level = loggingEvent.Level.DisplayName;
            _loggingInfo.hostName = Environment.MachineName;
            _loggingInfo.process = _currentProcess.ProcessName;
            _loggingInfo.threadName = loggingEvent.Properties["LoggingThread"] ?? loggingEvent.ThreadName;
            _loggingInfo.loggerName = loggingEvent.LoggerName;

            //handling messages
            object _objInfo = null;
            string _message = getMessageAndObjectInfo(loggingEvent, out _objInfo);

            if (_message != string.Empty)
            {
                _loggingInfo.message = _message;
            }

            if (_objInfo != null)
            {
                _loggingInfo.objectInfo = _objInfo;
            }
            
            //handling exceptions
            dynamic _exceptionInfo = getExceptionInfo(loggingEvent);
            if (_exceptionInfo != null)
            {
                _loggingInfo.exceptionObject = _exceptionInfo;
            }

            var ndcStack = log4net.ThreadContext.Stacks["NDC"];
            if (ndcStack != null && ndcStack.Count > 0)
            {
                string[] ndcStackArray = new string[ndcStack.Count];
                for (int n = ndcStack.Count - 1; n >= 0; n--)
                {
                    ndcStackArray[n] = ndcStack.Pop();
                }
                
                //TODO: we should find other way to get the elements of the stack
                //as pushing back is not a good option
                foreach (string stackValue in ndcStackArray)
                {
                    ndcStack.Push(stackValue);
                }
                _loggingInfo.ndcStack = ndcStackArray;
            }
            
            return _loggingInfo;
		}

        /// <summary>
        /// Returns the exception information. Also takes care of the InnerException.  
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private object getExceptionInfo(LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject == null)
                return null;

            dynamic exceptionInfo = new ExpandoObject();
            exceptionInfo.exceptionType = loggingEvent.ExceptionObject.GetType().FullName;
            exceptionInfo.exceptionMessage = loggingEvent.ExceptionObject.Message;
            exceptionInfo.stacktrace = loggingEvent.ExceptionObject.StackTrace;

            //most of the times dotnet exceptions contain important messages in the inner exceptions
            if (loggingEvent.ExceptionObject.InnerException != null)
            {
                dynamic innerException = new
                {
                    innerExceptionType = loggingEvent.ExceptionObject.InnerException.GetType().FullName,
                    innerExceptionMessage = loggingEvent.ExceptionObject.InnerException.Message,
                    innerStacktrace = loggingEvent.ExceptionObject.InnerException.StackTrace
                };
                exceptionInfo.innerException = innerException;
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
                objInfo = null;
            }
            else
            {
                objInfo = loggingEvent.MessageObject;
            }
            return message;
        }
    }
}
