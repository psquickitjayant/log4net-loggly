using System;
using System.Threading;
using log4net;

namespace log4net_loggly_console
{
	class Program
	{
        static void Main(string[] argArray)
        {
            log4net.Config.XmlConfigurator.Configure();

            var log = LogManager.GetLogger(typeof(Program));

            Thread thread = Thread.CurrentThread;
            thread.Name = "Main Thread";
            log.Info("Thread test");
            Console.WriteLine("thread name");

            log.Error("oops", new ArgumentOutOfRangeException("argArray"));
            Console.WriteLine("oops");
            
            log.Warn("hmmm", new ApplicationException("app exception"));
            Console.WriteLine("app exception");
            
            log.Info("yawn");
            Console.WriteLine("yawn");
            
            log.Debug("zzzz");
            Console.WriteLine("zzzz");
            
            log.InfoFormat("Loggly is the best {0} to collect Logs.", "service");
            Console.WriteLine("Loggly is the best {0} to collect Logs");
            
            log.Info(new { type = "newcustomtype", value = "newcustomvalue"});
            Console.WriteLine("customtYpe");
            
            log.Info(new TestObject());
            Console.WriteLine("test object");
            
            Console.ReadKey();
        }
	}
}
