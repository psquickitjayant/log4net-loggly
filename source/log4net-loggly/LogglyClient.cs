using System;
using System.Net;
using System.Text;

namespace log4net.loggly
{
	public class LogglyClient : ILogglyClient
	{
		public virtual void Send(ILogglyAppenderConfig config, string inputKey,string userAgent, string tag, string message)
		{
            string _tag = string.IsNullOrWhiteSpace(tag) ? config.Tag : tag;
            
            //keeping userAgent backward compatible
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                _tag = _tag + "," + userAgent;
            }

			var bytes = Encoding.UTF8.GetBytes(message);
    			var request = CreateWebRequest(config, _tag);
			
            using (var dataStream = request.GetRequestStream())
			{
				dataStream.Write(bytes, 0, bytes.Length);
				dataStream.Flush();
				dataStream.Close();
			}
			var response = request.GetResponse();
			response.Close();
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

            //adding x-forwarded-for header
            request.Headers.Add("x-forwarded-for", GetIPAddressOfMachine());

			return request;
		}

        /// <summary>
        /// Returns the IP Address of the Client
        /// </summary>
        /// <returns>IP Address</returns>
        private string GetIPAddressOfMachine()
        {
            var addressess = Dns.GetHostAddresses(Environment.MachineName);
            foreach (var addrs in addressess)
            {
                if (addrs.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return addrs.ToString();
                }
            }

            //If no IP Address is found then return machine name
            return Environment.MachineName;
        }
	}
}
