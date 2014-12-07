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


            log.Error("oops", new ArgumentOutOfRangeException("argArray"));
            using (log4net.ThreadContext.Stacks["NDC"].Push("STACKVALUE1"))
            {
                log.Warn("hmmm", new ApplicationException("app exception"));
                using (log4net.ThreadContext.Stacks["NDC"].Push("STACKVALUE2"))
                {
                    log.Info("yawn");
                    log.Debug("zzzz");
                }
                log.InfoFormat("Loggly is the best {0} to collect Logs.", "service");
                log.Info(new { type = "newcustomtype", value = "newcustomvalue" });
                log.Info(new TestObject());
            }

            Thread thread = Thread.CurrentThread;
            thread.Name = "Main Thread";
            log.Info("Thread test");
        }
    }
}
