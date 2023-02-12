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
SerilogJsonFormatter performs about the same as the CompactJsonFormatter

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.2364 (21H2)
Intel Core i7-9850H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT


```
|             Method |        FormatterType | LogEvents |         Mean |       Error |      StdDev |    Gen 0 |    Gen 1 |   Gen 2 | Allocated |
|------------------- |--------------------- |---------- |-------------:|------------:|------------:|---------:|---------:|--------:|----------:|
|        **Format_Text** | **SerilogJsonFormatter** |         **1** |     **375.7 ns** |     **2.69 ns** |     **2.25 ns** |   **0.1645** |   **0.0005** |       **-** |   **1,032 B** |
| Format_OneVariable | SerilogJsonFormatter |         1 |     482.3 ns |     9.46 ns |     9.29 ns |   0.1745 |   0.0005 |       - |   1,096 B |
|      Format_Object | SerilogJsonFormatter |         1 |   1,066.0 ns |    20.63 ns |    21.19 ns |   0.3338 |   0.0019 |       - |   2,096 B |
|   Format_Exception | SerilogJsonFormatter |         1 |   1,166.2 ns |    15.23 ns |    14.24 ns |   0.3490 |   0.0019 |       - |   2,192 B |
|        **Format_Text** | **SerilogJsonFormatter** |        **20** |   **5,841.1 ns** |   **115.54 ns** |   **123.62 ns** |   **1.9836** |   **0.0687** |       **-** |  **12,448 B** |
| Format_OneVariable | SerilogJsonFormatter |        20 |   7,520.8 ns |    70.76 ns |    62.72 ns |   2.1744 |   0.0839 |       - |  13,648 B |
|      Format_Object | SerilogJsonFormatter |        20 |  18,795.1 ns |   368.45 ns |   424.31 ns |   4.9133 |   0.3357 |       - |  30,985 B |
|   Format_Exception | SerilogJsonFormatter |        20 |  20,366.5 ns |   160.97 ns |   134.42 ns |   5.2185 |   1.2512 |       - |  32,786 B |
|        **Format_Text** | **SerilogJsonFormatter** |        **50** |  **14,709.9 ns** |   **258.98 ns** |   **242.25 ns** |   **4.3640** |   **0.5188** |       **-** |  **27,448 B** |
| Format_OneVariable | SerilogJsonFormatter |        50 |  18,695.7 ns |   296.06 ns |   276.94 ns |   6.6223 |   0.0305 |       - |  41,528 B |
|      Format_Object | SerilogJsonFormatter |        50 |  44,800.1 ns |   780.26 ns |   729.85 ns |  13.7939 |  13.4888 |       - |  87,003 B |
|   Format_Exception | SerilogJsonFormatter |        50 |  51,082.6 ns |   519.72 ns |   460.72 ns |  14.5264 |  14.2212 |       - |  91,502 B |
|        **Format_Text** | **SerilogJsonFormatter** |       **500** | **145,847.8 ns** | **1,584.36 ns** | **1,404.49 ns** |  **52.9785** |  **32.9590** | **26.1230** | **260,271 B** |
| Format_OneVariable | SerilogJsonFormatter |       500 | 206,189.4 ns | 2,668.00 ns | 2,620.33 ns |  36.8652 |  36.8652 | 36.8652 | 322,340 B |
|      Format_Object | SerilogJsonFormatter |       500 | 480,767.4 ns | 3,665.10 ns | 3,249.01 ns | 141.6016 | 117.1875 | 63.9648 | 712,983 B |
|   Format_Exception | SerilogJsonFormatter |       500 | 552,242.9 ns | 8,142.88 ns | 7,218.45 ns | 151.3672 | 124.0234 | 66.4063 | 806,279 B |
|        **Format_Text** | **CompactJsonFormatter** |         **1** |     **315.7 ns** |     **6.01 ns** |     **6.69 ns** |   **0.1326** |        **-** |       **-** |     **832 B** |
| Format_OneVariable | CompactJsonFormatter |         1 |     440.2 ns |     5.21 ns |     4.87 ns |   0.1760 |   0.0005 |       - |   1,104 B |
|      Format_Object | CompactJsonFormatter |         1 |   1,208.5 ns |    17.01 ns |    15.91 ns |   0.3796 |   0.0019 |       - |   2,392 B |
|   Format_Exception | CompactJsonFormatter |         1 |   1,182.6 ns |    23.30 ns |    60.55 ns |   0.3891 |   0.0019 |       - |   2,448 B |
|        **Format_Text** | **CompactJsonFormatter** |        **20** |   **4,659.0 ns** |    **89.86 ns** |    **79.66 ns** |   **1.6937** |   **0.0534** |       **-** |  **10,632 B** |
| Format_OneVariable | CompactJsonFormatter |        20 |   6,780.9 ns |    38.21 ns |    35.74 ns |   2.2507 |   0.1144 |       - |  14,144 B |
|      Format_Object | CompactJsonFormatter |        20 |  20,908.2 ns |   163.20 ns |   144.68 ns |   5.9509 |   1.4343 |       - |  37,337 B |
|   Format_Exception | CompactJsonFormatter |        20 |  20,276.6 ns |   209.90 ns |   175.28 ns |   7.5684 |   1.8311 |       - |  47,571 B |
|        **Format_Text** | **CompactJsonFormatter** |        **50** |  **11,351.6 ns** |    **76.21 ns** |    **63.63 ns** |   **4.6387** |   **0.5646** |       **-** |  **29,200 B** |
| Format_OneVariable | CompactJsonFormatter |        50 |  16,393.9 ns |   102.15 ns |    90.56 ns |   5.1270 |   1.2207 |       - |  32,200 B |
|      Format_Object | CompactJsonFormatter |        50 |  53,281.1 ns |   828.32 ns |   774.81 ns |  16.4795 |  16.1133 |       - | 103,907 B |
|   Format_Exception | CompactJsonFormatter |        50 |  49,100.0 ns |   412.84 ns |   344.74 ns |  16.9678 |  16.6626 |       - | 106,912 B |
|        **Format_Text** | **CompactJsonFormatter** |       **500** | **111,204.9 ns** |   **690.92 ns** |   **612.48 ns** |  **41.6260** |  **41.5039** |       **-** | **261,944 B** |
| Format_OneVariable | CompactJsonFormatter |       500 | 182,440.2 ns | 2,176.24 ns | 1,929.18 ns |  63.2324 |  41.0156 | 25.3906 | 324,129 B |
|      Format_Object | CompactJsonFormatter |       500 | 561,303.1 ns | 5,199.15 ns | 4,863.28 ns | 164.0625 | 106.4453 | 53.7109 | 902,743 B |
|   Format_Exception | CompactJsonFormatter |       500 | 523,376.6 ns | 4,370.27 ns | 3,649.38 ns | 179.6875 | 122.0703 | 63.4766 | 964,989 B |
