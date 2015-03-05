log4net-loggly
==============

Custom log4net appenders for importing logging events to loggly. It’s asynchronous and will send logs in the background without blocking your application. Check out Loggly's [.Net logging documentation](https://www.loggly.com/docs/net-logs/) to learn more.

Download log4net-loggly package from NuGet. Use the following command.

    Install-Package log4net-loggly

Add the following code in your web.config to configure LogglyAppender in your application

    <configSections>
      <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
    </configSections>
    <log4net>
      <root>
        <level value="ALL" />
        <appender-ref ref="LogglyAppender" />
      </root>
      <appender name="LogglyAppender" type="log4net.loggly.LogglyAppender, log4net-loggly">
        <rootUrl value="http://logs-01.loggly.com/" />
        <inputKey value="your-customer-token" />
		<tag value="your-custom-tag" />
      </appender>
    </log4net>
    
Add the following entry in your AssemblyInfo.cs
```
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
```

Alternatively, you can add the following code in your Main method or in Global.asax file

```
log4net.Config.XmlConfigurator.Configure();
```

Create an object of the Log class using LogManager

    var logger = LogManager.GetLogger(typeof(Class));
    
Send logs to Loggly using the following code

```  
    logger.Info("log message");
```

<strong>For Console Application</strong>

You should add the following statement at the end of your Main method as the log4net-loggly library is asynchronous so there needs to be time for the threads the complete logging before the application exits.

```
Console.ReadKey();
```
