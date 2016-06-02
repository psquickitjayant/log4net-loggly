log4net-loggly
==============

Custom log4net appenders for importing logging events to loggly. It’s asynchronous and will send logs in the background without blocking your application. Check out Loggly's [.Net logging documentation](https://www.loggly.com/docs/net-logs/) to learn more.

Download log4net-loggly package from NuGet. Use the following command.

    Install-Package log4net-loggly

Add the following code in your web.config to configure LogglyAppender in your application
```
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
		<logicalThreadContextKeys value="lkey1,lkey2" /> <!-- optional -->
		<globalContextKeys value="gkey1,gkey2" /> <!-- optional -->
      </appender>
    </log4net>
```    
To send **GlobalContext** and **LogicalThreadContext** properties in your log you need define the list of used properties in the configuration. 

For GlobalContext Properties use 
```<globalContextKeys value="gkey1,gkey2" />```

For LogicalThreadContext Properties 
```<logicalThreadContextKeys value="lkey1,lkey2" />```


You can also use **layout** with in the Config to render logs according to your Pattern Layouts

     <layout type="log4net.Layout.PatternLayout">
         <conversionPattern value="%date [%thread] %-5level %logger %message" />
     </layout>

By default, library uses Loggly /bulk end point (https://www.loggly.com/docs/http-bulk-endpoint/). To use /inputs endpoint, add the following configuration in config file.

```
<logMode value="inputs" />
```


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
