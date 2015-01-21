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

            Thread newThread1 = new Thread(() =>
            {
                Thread curntThread = Thread.CurrentThread;
                curntThread.Name = "Inner thread";
                log.Info("this is an inner thread");

                using (ThreadContext.Stacks["NDC"].Push("stacktestkey1"))
                {
                    log.Info("new stack test key 1");
                    log.Info("new stack1 test key 2");
                }
                
                using (ThreadContext.Stacks["NDC"].Push("stacktestkey2"))
                {
                    log.Info("new stack test key 2");
                }
            });

            newThread1.Start();

            Thread newThread2 = new Thread(() =>
            {
                Thread curntThread = Thread.CurrentThread;
                curntThread.Name = "Inner thread 2";

                log.Info("this is an inner thread 2");

                using (ThreadContext.Stacks["NDC"].Push("stacktestkey3"))
                {
                    log.Info("new stack test key 3");
                    log.Info("new stack2 test key 3");
                }

                using (ThreadContext.Stacks["NDC"].Push("stacktestkey4"))
                {
                    log.Info("new stack test key 4");
                }
            });

            newThread2.Start();

            using (log4net.ThreadContext.Stacks["NDC"].Push("STACKVALUE1"))
            {
                log.Debug("zzzz");
                log.InfoFormat("Loggly is the best {0} to collect Logs.", "service");
            }

            using (log4net.ThreadContext.Stacks["NDC"].Push("STACKVALUE2"))
            {
                log.Info(new { type = "newcustomtype", value = "newcustomvalue" });
                log.Info("StackValue2 message");
            }
            
            log.Info(new TestObject());
            Console.ReadKey();
        }
	}
}
