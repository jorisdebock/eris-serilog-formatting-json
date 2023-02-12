# Eris.Serilog.Formatting.Json

Json formatter for Serilog based on the [serilog-formatting-compact](https://github.com/serilog/serilog-formatting-compact) formatter

Differences with the compact logger
- Use the fullname for the properties e.g. @timestamp instead of @t, message instead of @mt. The names can be configured via the settings.
- Always log the log level, does not assume Information as default
- Render the properties camelCase in the json format
- Render the properties one time, normal or formatted instead of the @r
- Message can optionally be rendered, default is to use the message template and not render
- Properties can optionally be wrapped in a parent object, default does not wrap

## Getting started

### create logger with SerilogJsonFormatter

```c#
var logger = new LoggerConfiguration()
    .WriteTo.Console(new SerilogJsonFormatter())
    .CreateLogger();
```

### Settings

pass the settings object to the formatter
```
var settings = new SerilogJsonFormatterSettings 
{
...
}

new SerilogJsonFormatter(settings);
```

|Name          |Default value|
|--------------|-------------|
|TimestampName |@timestamp   |
|LogLevelName  |level        |
|MessageName   |message      |
|ExceptionName |exception    |
|PropertiesName|properties   |
|RenderMessage |false        |
|WrapProperties|false        |

## Sample
see sample folder for the sample code

log messages
```c#
logger.Information("Hello, Foo");
```
```json
{"@timestamp":"2023-02-12T15:21:51.1626151Z","level":"Information","message":"Hello, Foo"}
```
```c#
logger.Information("Hello, {Name}", user.Name);
```
```json
{"@timestamp":"2023-02-12T15:21:51.1778631Z","level":"Information","message":"Hello, {Name}","name":"Foo"}
```
```c#
var user = new User("Foo", 22, new[] { 1, 2, 3 }, 0xffffff);
logger.Information("Hello, {@User}", user);
```
```json
{"@timestamp":"2023-02-12T15:21:51.1812302Z","level":"Information","message":"Hello, {@User}","user":{"name":"Foo","age":22,"tags":[1,2,3],"color":16777215}}
```
```c#
logger.Information("Hello, Foo");
```
```json
{"@timestamp":"2023-02-12T15:21:51.1626151Z","level":"Information","message":"Hello, Foo"}
```
```c#
logger.Error(new Exception("exception message", new Exception("inner exception message")), "error");
```
```json
{"@timestamp":"2023-02-12T15:21:51.1876463Z","level":"Error","message":"error","exception":"System.Exception: exception message\r\n ---> System.Exception: inner exception message\r\n   --- End of inner exception stack trace ---"}
```

RenderMessage: true
```c#
logger.Information("Hello, Foo");
```
```json
{"@timestamp":"2023-02-12T15:21:51.1891336Z","level":"Information","message":"Hello, \"Foo\"","name":"Foo"}
```

WrapProperties: true
```c#
logger.Information("Hello, Foo");
```
```json
{"@timestamp":"2023-02-12T15:21:51.1884321Z","level":"Information","message":"Hello, {Name}","properties":{"name":"Foo"}}
```

Microsoft.Extensions.Logging.ILogger
```c#
var builder = Host.CreateDefaultBuilder(args);
builder.UseSerilog(logger);
var app = builder.Build();

var mlogger = app.Services.GetRequiredService<ILogger<User>>();
```

```c#
mlogger.LogInformation("Hello, Foo");
```
```json
{"@timestamp":"2023-02-12T15:21:51.4978918Z","level":"Information","message":"Hello, Foo","sourceContext":"User"}
```
```c#
mlogger.LogInformation("Hello, {Name}", user.Name);
```
```json
{"@timestamp":"2023-02-12T15:21:51.5016522Z","level":"Information","message":"Hello, {Name}","name":"Foo","sourceContext":"User"}
```
```c#
mlogger.LogInformation("Hello, {@User}", user);
```
```json
{"@timestamp":"2023-02-12T15:21:51.5019571Z","level":"Information","message":"Hello, {@User}","user":{"name":"Foo","age":22,"tags":[1,2,3],"color":16777215},"sourceContext":"User"}
```
```c#
mlogger.LogInformation("Color: {Color:x}", user.Color);
```
```json
{"@timestamp":"2023-02-12T15:21:51.5021091Z","level":"Information","message":"Color: {Color:x}","color":"ffffff","sourceContext":"User"}
```
```c#
mlogger.LogError(new Exception("exception message", new Exception("inner exception message")), "error");
```
```json
{"@timestamp":"2023-02-12T15:21:51.5023791Z","level":"Error","message":"error","exception":"System.Exception: exception message\r\n ---> System.Exception: inner exception message\r\n   --- End of inner exception stack trace ---","sourceContext":"User"}
```

## Benchmarks
