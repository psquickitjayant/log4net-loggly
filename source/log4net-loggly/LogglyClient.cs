using System;
using System.Net;
using System.Text;

namespace log4net.loggly
{
	public class LogglyClient : ILogglyClient
	{
		public virtual void Send(ILogglyAppenderConfig config, string message)
		{
            int maxRetryAllowed = 5;
            int totalRetries = 0;

            string _tag = config.Tag;
            
            //keeping userAgent backward compatible
            if (!string.IsNullOrWhiteSpace(config.UserAgent))
            {
                _tag = _tag + "," + config.UserAgent;
            }

            while (totalRetries < maxRetryAllowed)
            {
                totalRetries++;
                try
                {
			        var bytes = Encoding.UTF8.GetBytes(message);
                    var webRequest = CreateWebRequest(config, _tag);

                    using (var dataStream = webRequest.GetRequestStream())
			        {
				        dataStream.Write(bytes, 0, bytes.Length);
				        dataStream.Flush();
				        dataStream.Close();
			        }
            
                    var webResponse = webRequest.GetResponse();
                    webResponse.Close();
                    break;
                }
                catch { }
            }
		}

		protected virtual HttpWebRequest CreateWebRequest(ILogglyAppenderConfig config, string tag)
		{
			var url = String.Concat(config.RootUrl, config.InputKey);
	        //adding userAgent as tag in the log
	        url = String.Concat(url, "/tag/" + tag);
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = "POST";
			request.ReadWriteTimeout = request.Timeout = config.TimeoutInSeconds * 1000;
			request.UserAgent = config.UserAgent;
			request.KeepAlive = true;
			request.ContentType = "application/json";
			return request;
		}
	}
}
