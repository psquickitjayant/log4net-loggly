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
        private ILogglyAppenderConfig _config;

        public LogglyFormatter()
        {
            _currentProcess = Process.GetCurrentProcess();
        }

        public virtual void AppendAdditionalLoggingInformation(ILogglyAppenderConfig config, LoggingEvent loggingEvent)
        {
            this._config = config;
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
                    if ((ThreadContext.Properties[key] as IFixingRequired) != null
                        && (ThreadContext.Properties[key] as IFixingRequired).GetFixedObject() != null)
                    {
                        p[key] = (ThreadContext.Properties[key] as IFixingRequired).GetFixedObject();
                    }
                    else
                    {
                        p[key] = ThreadContext.Properties[key].ToString();
                    }
                }
            }

            //handling logicalthreadcontext properties
            if (this._config.LogicalThreadContextKeys != null)
            {
                var ltp = _loggingInfo as IDictionary<string, object>;
                string[] _LogicalThreadContextProperties = this._config.LogicalThreadContextKeys.Split(',');
                foreach (string key in _LogicalThreadContextProperties)
                {
                    if (LogicalThreadContext.Properties[key] != null)
                    {
                        if ((LogicalThreadContext.Properties[key] as IFixingRequired) != null
                            && (LogicalThreadContext.Properties[key] as IFixingRequired).GetFixedObject() != null)
                        {
                            ltp[key] = (LogicalThreadContext.Properties[key] as IFixingRequired).GetFixedObject();
                        }
                        else
                        {
                            ltp[key] = LogicalThreadContext.Properties[key].ToString();
                        }
                    }
                }
            }

            //handling globalcontext properties
            if (this._config.GlobalContextKeys != null)
            {
                var gcp = _loggingInfo as IDictionary<string, object>;
                string[] _globalContextProperties = this._config.GlobalContextKeys.Split(',');
                foreach (string key in _globalContextProperties)
                {
                    if (GlobalContext.Properties[key] != null)
                    {
                        if ((GlobalContext.Properties[key] as IFixingRequired) != null
                            && (GlobalContext.Properties[key] as IFixingRequired).GetFixedObject() != null)
                        {
                            gcp[key] = (GlobalContext.Properties[key] as IFixingRequired).GetFixedObject();
                        }
                        else
                        {
                            gcp[key] = GlobalContext.Properties[key].ToString();
                        }
                    }
                }
            }

            string jsonMessage = string.Empty;
            if (TryGetParsedJsonFromLog(_loggingInfo, _loggedObject, out jsonMessage))
            {
                return jsonMessage;
            }
            else
            {
                //converting event info to Json string
                return JsonConvert.SerializeObject(_loggingInfo,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });
            }
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
            _loggingInfo.timestamp = timeStamp.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffzzz");

            string jsonMessage = string.Empty;
            if (TryGetParsedJsonFromLog(_loggingInfo, log, out jsonMessage))
            {
                return jsonMessage;
            }
            else
            {
                _loggingInfo.message = log;
                return JsonConvert.SerializeObject(_loggingInfo,
                    new JsonSerializerSettings()
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    });
            }
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
        /// Tries to merge log with the logged object or rendered log
        /// and converts to JSON
        /// </summary>
        /// <param name="loggingInfo"></param>
        /// <param name="loggingObject"></param>
        /// <param name="_loggingEventJSON"></param>
        /// <returns></returns>
        private bool TryGetParsedJsonFromLog(dynamic loggingInfo, object loggingObject, out string _loggingEventJSON)
        {
            //serialize the dynamic object to string
            _loggingEventJSON = JsonConvert.SerializeObject(loggingInfo,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            }); 
            
            //if loggingObject is null then we need to go to further step
            if (loggingObject == null)
                return false;
            
            try
            {
                string _loggedObjectJSON = string.Empty;
                if (loggingObject.GetType() == typeof(string))
                {
                    _loggedObjectJSON = loggingObject.ToString();
                }
                else
                {
                    _loggedObjectJSON = JsonConvert.SerializeObject(loggingObject,
                        new JsonSerializerSettings()
                        {
                            PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        });
                }

                //try to parse the logging object
                JObject jObject = JObject.Parse(_loggedObjectJSON);
                JObject jEvent = JObject.Parse(_loggingEventJSON);

                //merge these two objects into one JSON string
                jEvent.Merge(jObject, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });

                _loggingEventJSON = jEvent.ToString();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
