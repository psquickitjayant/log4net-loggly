using System;
using System.Collections.Generic;
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
            ThreadContext.Properties["MainThreadContext"] = "MainThreadContextValue";
            log.Info("Thread test");
            log.Error("oops", new ArgumentOutOfRangeException("argArray"));
            log.Warn("hmmm", new ApplicationException("app exception"));
            log.Info("yawn");
            

            Thread newThread1 = new Thread(() =>
            {
                Thread curntThread = Thread.CurrentThread;
                curntThread.Name = "Inner thread 1";
                ThreadContext.Properties["InnerThread1Context"] = "InnerThreadContext1Values";
                log.Info("this is an inner thread 1");
            });

            newThread1.Start();

            Thread newThread2 = new Thread(() =>
            {
                Thread curntThread = Thread.CurrentThread;
                curntThread.Name = "Inner thread 2";
                ThreadContext.Properties["InnerThread2Context"] = "InnerThreadContext2Values";
                log.Info("this is an inner thread 2");
            });

            newThread2.Start();
			//Test self referencing
			var parent = new Person { Name = "John Smith" };
			var child1 = new Person { Name = "Bob Smith", Parent = parent };
			var child2 = new Person { Name = "Suzy Smith", Parent = parent };
			parent.Children = new List<Person> { child1, child2 };
			log.Info(parent);

            log.Debug("zzzz");
            log.InfoFormat("Loggly is the best {0} to collect Logs.", "service");
            log.Info(new { type = "newcustomtype", value = "newcustomvalue" });
            log.Info(new TestObject());
            Console.ReadKey();
        }
	}
}
