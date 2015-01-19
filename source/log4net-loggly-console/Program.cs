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
            log.Error("oops", new ArgumentOutOfRangeException("argArray"));
            log.Warn("hmmm", new ApplicationException("app exception"));
            log.Info("yawn");

            using (log4net.ThreadContext.Stacks["NDC"].Push("STACKVALUE1"))
            {
                log.Debug("zzzz");
                using (log4net.ThreadContext.Stacks["NDC"].Push("STACKVALUE2"))
                {
                    log.Info(new { type = "newcustomtype", value = "newcustomvalue" });
                    log.Info("StackValue2 message");
                }
                log.InfoFormat("Loggly is the best {0} to collect Logs.", "service");
            }

            log.Info(new TestObject());
            Console.ReadKey();
        }
	}
}
