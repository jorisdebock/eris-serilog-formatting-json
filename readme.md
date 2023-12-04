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
|LogTraceId    |true         |
|LogSpanId     |true         |

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
SerilogJsonFormatter performs about the same as the CompactJsonFormatter

``` ini

BenchmarkDotNet v0.13.10, Windows 10 (10.0.19044.3448/21H2/November2021Update)
Intel Core i7-9850H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 8.0.100
  [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

```

| Method             | FormatterType        | LogEvents | Mean         | Error        | StdDev        | Median       | Gen0     | Gen1     | Gen2    | Allocated |
|------------------- |--------------------- |---------- |-------------:|-------------:|--------------:|-------------:|---------:|---------:|--------:|----------:|
| Format_Text        | SerilogJsonFormatter | 1         |     381.0 ns |      8.31 ns |      24.25 ns |     376.9 ns |   0.1554 |        - |       - |     976 B |
| Format_OneVariable | SerilogJsonFormatter | 1         |     481.1 ns |     11.17 ns |      32.95 ns |     469.6 ns |   0.1745 |        - |       - |    1096 B |
| Format_Object      | SerilogJsonFormatter | 1         |   1,011.2 ns |     19.64 ns |      35.92 ns |   1,012.5 ns |   0.3195 |   0.0010 |       - |    2008 B |
| Format_Exception   | SerilogJsonFormatter | 1         |   1,078.1 ns |     21.49 ns |      39.30 ns |   1,079.7 ns |   0.3490 |        - |       - |    2192 B |
| Format_Text        | SerilogJsonFormatter | 20        |   5,087.4 ns |     99.84 ns |     164.05 ns |   5,085.4 ns |   1.8005 |   0.0381 |       - |   11328 B |
| Format_OneVariable | SerilogJsonFormatter | 20        |   7,931.1 ns |    257.86 ns |     731.51 ns |   7,749.6 ns |   2.1667 |   0.0458 |       - |   13648 B |
| Format_Object      | SerilogJsonFormatter | 20        |  25,178.7 ns |  1,875.02 ns |   5,528.53 ns |  24,961.9 ns |   4.6387 |   0.1831 |       - |   29225 B |
| Format_Exception   | SerilogJsonFormatter | 20        |  21,239.5 ns |    683.76 ns |   1,961.85 ns |  20,530.9 ns |   5.2185 |   0.2136 |       - |   32787 B |
| Format_Text        | SerilogJsonFormatter | 50        |  15,383.5 ns |    885.10 ns |   2,553.71 ns |  14,728.8 ns |   3.9063 |   0.1526 |       - |   24648 B |
| Format_OneVariable | SerilogJsonFormatter | 50        |  18,897.2 ns |    504.51 ns |   1,431.22 ns |  18,685.7 ns |   6.6223 |   0.4578 |       - |   41528 B |
| Format_Object      | SerilogJsonFormatter | 50        |  46,688.0 ns |  2,046.62 ns |   5,970.08 ns |  44,648.9 ns |  13.1226 |   1.4648 |       - |   82603 B |
| Format_Exception   | SerilogJsonFormatter | 50        |  48,251.3 ns |  1,090.71 ns |   2,985.80 ns |  47,649.7 ns |  14.5264 |   1.5869 |       - |   91503 B |
| Format_Text        | SerilogJsonFormatter | 500       | 215,682.3 ns | 22,361.17 ns |  65,932.41 ns | 188,325.3 ns | 145.2637 | 145.2637 | 24.4141 |  232194 B |
| Format_OneVariable | SerilogJsonFormatter | 500       | 245,185.3 ns |  4,903.25 ns |  12,830.94 ns | 240,746.9 ns | 187.2559 | 187.2559 | 31.0059 |  322340 B |
| Format_Object      | SerilogJsonFormatter | 500       | 636,877.6 ns | 53,041.11 ns | 155,560.40 ns | 562,637.4 ns | 233.3984 | 205.0781 | 58.5938 |  668888 B |
| Format_Exception   | SerilogJsonFormatter | 500       | 647,901.7 ns | 19,332.29 ns |  55,778.06 ns | 634,147.8 ns | 273.4375 | 239.2578 | 68.3594 |  806203 B |
| Format_Text        | CompactJsonFormatter | 1         |     284.9 ns |      6.68 ns |      18.40 ns |     281.9 ns |   0.1235 |        - |       - |     776 B |
| Format_OneVariable | CompactJsonFormatter | 1         |     519.0 ns |     34.81 ns |      97.05 ns |     491.8 ns |   0.1760 |        - |       - |    1104 B |
| Format_Object      | CompactJsonFormatter | 1         |   1,149.7 ns |     37.94 ns |     107.62 ns |   1,119.3 ns |   0.3605 |        - |       - |    2272 B |
| Format_Exception   | CompactJsonFormatter | 1         |   1,126.6 ns |     49.47 ns |     141.95 ns |   1,104.8 ns |   0.3376 |        - |       - |    2120 B |
| Format_Text        | CompactJsonFormatter | 20        |   6,908.4 ns |    306.50 ns |     903.72 ns |   6,954.6 ns |   1.5106 |   0.0153 |       - |    9512 B |
| Format_OneVariable | CompactJsonFormatter | 20        |   9,918.2 ns |    736.64 ns |   2,171.99 ns |  10,165.2 ns |   2.2430 |   0.0305 |       - |   14144 B |
| Format_Object      | CompactJsonFormatter | 20        |  28,127.1 ns |  1,900.17 ns |   5,602.71 ns |  27,346.2 ns |   5.5542 |   0.1831 |       - |   34938 B |
| Format_Exception   | CompactJsonFormatter | 20        |  19,724.9 ns |    660.17 ns |   1,840.28 ns |  19,231.8 ns |   6.5308 |   0.3967 |       - |   41011 B |
| Format_Text        | CompactJsonFormatter | 50        |  13,160.9 ns |    719.90 ns |   2,122.63 ns |  12,680.9 ns |   4.1962 |   0.1373 |       - |   26400 B |
| Format_OneVariable | CompactJsonFormatter | 50        |  18,262.8 ns |  1,098.06 ns |   3,115.02 ns |  17,316.3 ns |   5.1270 |   0.1526 |       - |   32200 B |
| Format_Object      | CompactJsonFormatter | 50        |  60,516.0 ns |  3,507.32 ns |  10,175.36 ns |  59,008.9 ns |  15.5640 |   1.6479 |       - |   97908 B |
| Format_Exception   | CompactJsonFormatter | 50        |  49,796.8 ns |  2,215.16 ns |   6,426.57 ns |  47,972.5 ns |  14.4043 |   1.4648 |       - |   90511 B |
| Format_Text        | CompactJsonFormatter | 500       | 125,132.4 ns | 10,227.90 ns |  30,157.20 ns | 120,205.1 ns |  36.9873 |   6.1035 |       - |  233945 B |
| Format_OneVariable | CompactJsonFormatter | 500       | 246,166.1 ns | 17,943.89 ns |  52,907.98 ns | 238,416.7 ns |  53.7109 |  26.8555 | 26.8555 |  324099 B |
| Format_Object      | CompactJsonFormatter | 500       | 826,247.2 ns | 69,321.31 ns | 204,395.45 ns | 736,404.3 ns | 123.0469 |  60.5469 | 60.5469 |  842640 B |
| Format_Exception   | CompactJsonFormatter | 500       | 670,279.6 ns | 29,375.27 ns |  86,613.65 ns | 643,522.6 ns | 142.5781 |  71.2891 | 71.2891 |  800832 B |
