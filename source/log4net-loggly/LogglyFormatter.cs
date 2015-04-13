using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net.Core;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Linq;

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
            return PreParse(loggingEvent);
        }

        public virtual string ToJson(IEnumerable<LoggingEvent> loggingEvents)
        {
            return JsonConvert.SerializeObject(loggingEvents.Select(PreParse),new JsonSerializerSettings(){
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        public virtual string ToJson(string renderedLog, DateTime timeStamp)
        {
            return ParseRenderedLog(renderedLog, timeStamp);
        }

        /// <summary>
        /// Formats the log event to various JSON fields that are to be shown in Loggly.
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private string PreParse(LoggingEvent loggingEvent)
        {
            //formating base logging info
            dynamic _loggingInfo = new ExpandoObject();
            _loggingInfo.timestamp = loggingEvent.TimeStamp.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffzzz");
            _loggingInfo.level = loggingEvent.Level.DisplayName;
            _loggingInfo.hostName = Environment.MachineName;
            _loggingInfo.process = _currentProcess.ProcessName;
            _loggingInfo.threadName = loggingEvent.ThreadName;
            _loggingInfo.loggerName = loggingEvent.LoggerName;

            //handling messages
            object _loggedObject = null;
            string _message = GetMessageAndObjectInfo(loggingEvent, out _loggedObject);

            if (_message != string.Empty)
            {
                _loggingInfo.message = _message;
            }

            //handling exceptions
            dynamic _exceptionInfo = GetExceptionInfo(loggingEvent);
            if (_exceptionInfo != null)
            {
                _loggingInfo.exception = _exceptionInfo;
            }

            //handling threadcontext properties
            string[] _threadContextProperties = ThreadContext.Properties.GetKeys();
            if (_threadContextProperties != null && _threadContextProperties.Any())
            {
                var p = _loggingInfo as IDictionary<string, object>;
                foreach (string key in _threadContextProperties)
                {
                    //handling threadstack
                    if (ThreadContext.Properties[key].GetType() ==
                            typeof(log4net.Util.ThreadContextStack))
                    {
                        string[] stackArray;
                        if (IncludeThreadStackValues(ThreadContext.Properties[key]
                                    as log4net.Util.ThreadContextStack, out stackArray))
                        {
                            p[key] = stackArray;
                        }
                    }
                    else
                    {
                        p[key] = ThreadContext.Properties[key];
                    }
                }
            }

            //converting event info to Json string
            var _loggingEventJSON = JsonConvert.SerializeObject(_loggingInfo,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });

            //checking if _loggedObject is not null
            //if it is not null then convert it into JSON string
            //and concatenate to the _loggingEventJSON

            if (_loggedObject != null)
            {
                //converting passed object to JSON string

                var _loggedObjectJSON = JsonConvert.SerializeObject(_loggedObject,
                new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

                //concatenating _loggedObjectJSON with _loggingEventJSON
                //http://james.newtonking.com/archive/2014/08/04/json-net-6-0-release-4-json-merge-dependency-injection

                JObject jEvent = JObject.Parse(_loggingEventJSON);
                JObject jObject = JObject.Parse(_loggedObjectJSON);

                jEvent.Merge(jObject, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });

                _loggingEventJSON = jEvent.ToString();
            }

            return _loggingEventJSON;
        }

        /// <summary>
        /// Merged Rendered log and formatted timestamp in the single Json object
        /// </summary>
        /// <param name="log"></param>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private string ParseRenderedLog(string log, DateTime timeStamp)
        {
            dynamic _loggingInfo = new ExpandoObject();
            _loggingInfo.message = log;
            _loggingInfo.timestamp = timeStamp.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffzzz");

            //converting event info to Json string
            var _loggingEventJSON = JsonConvert.SerializeObject(_loggingInfo);

            return _loggingEventJSON;
        }

        /// <summary>
        /// Returns the exception information. Also takes care of the InnerException.  
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns></returns>
        private object GetExceptionInfo(LoggingEvent loggingEvent)
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
        private string GetMessageAndObjectInfo(LoggingEvent loggingEvent, out object objInfo)
        {
            string message = string.Empty;
            objInfo = null;

            if (loggingEvent.MessageObject != null)
            {
                if (loggingEvent.MessageObject.GetType() == typeof(string)
                        //if it is sent by using InfoFormat method then treat it as a string message
                        || loggingEvent.MessageObject.GetType().FullName == "log4net.Util.SystemStringFormat"
                        || loggingEvent.MessageObject.GetType().FullName.Contains("StringFormatFormattedMessage"))
                {
                    message = loggingEvent.MessageObject.ToString();
                }
                else
                {
                    objInfo = loggingEvent.MessageObject;
                }
            }
            else
            {
                //adding message as null so that the Loggly user
                //can know that a null object is logged.
                message = "null";
            }
            return message;
        }

        /// <summary>
        /// Returns whether to include stack array or not
        /// Also outs the stack array if needed to include
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="includeStackKey"></param>
        /// <returns></returns>
        private bool IncludeThreadStackValues(log4net.Util.ThreadContextStack stack,
        out string[] stackArray)
        {
            if (stack != null && stack.Count > 0)
            {
                stackArray = new string[stack.Count];
                for (int n = stack.Count - 1; n >= 0; n--)
                {
                    stackArray[n] = stack.Pop();
                }

                foreach (string stackValue in stackArray)
                {
                    stack.Push(stackValue);
                }
                return true;
            }
            else
            {
                stackArray = null;
                return false;
            }

        }
    }
}
