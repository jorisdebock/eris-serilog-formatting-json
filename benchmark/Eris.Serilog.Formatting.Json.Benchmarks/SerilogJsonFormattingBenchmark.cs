using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Parsing;

namespace Eris.Serilog.Formatting.Json;

[MemoryDiagnoser]
[MarkdownExporter]
public class SerilogFormattingBenchmark
{
    private static readonly LogEvent _logEventText = new(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("message"),
            Array.Empty<LogEventProperty>());

    private static readonly LogEvent _logEventOneVariable = new(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("message {Variable}"),
            new LogEventProperty[] { new LogEventProperty("Variable", new ScalarValue("value")) });

    private static readonly LogEvent _logEventException = new(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            new NullReferenceException("error message", new Exception("inner exception")),
            new MessageTemplateParser().Parse("message {Variable}"),
            new LogEventProperty[] { new LogEventProperty("Variable", new ScalarValue("value")) });

    private static readonly DateTimeOffset _logEventDateTimeOffset = new(2023, 7, 15, 12, 50, 30, TimeSpan.FromHours(2));
    private static readonly LogEvent _logEventObject = new(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Hello, {@User}, {N:x8} at {Now}"),
            new LogEventProperty[] 
            { 
                new LogEventProperty("@User", new ScalarValue(new { Name = "nblumhardt", Tags = new[] { 1, 2, 3 } })),
                new LogEventProperty("N", new ScalarValue(123)),
                new LogEventProperty("Now", new ScalarValue(_logEventDateTimeOffset)) 
            });

    [Params(FormatterType.SerilogJsonFormatter, FormatterType.CompactJsonFormatter)]
    public FormatterType FormatterType { get; set; }

    [Params(1, 20, 50, 500)]
    public int LogEvents { get; set; }

    private ITextFormatter _formatter;

    [GlobalSetup]
    public void Setup()
    {
        _formatter = FormatterType switch
        {
            FormatterType.SerilogJsonFormatter => new SerilogJsonFormatter(),
            FormatterType.CompactJsonFormatter => new CompactJsonFormatter(),
            _ => throw new NotImplementedException(),
        };
    }

    [Benchmark]
    public string Format_Text()
    {
        var output = new StringWriter();
        for (int i = 0; i < LogEvents; i++)
        {
            _formatter.Format(_logEventText, output);
        }
        return output.ToString();
    }

    [Benchmark]
    public string Format_OneVariable()
    {
        var output = new StringWriter();
        for (int i = 0; i < LogEvents; i++)
        {
            _formatter.Format(_logEventOneVariable, output);
        }
        return output.ToString();
    }

    [Benchmark]
    public string Format_Object()
    {
        var output = new StringWriter();
        for (int i = 0; i < LogEvents; i++)
        {
            _formatter.Format(_logEventObject, output);
        }
        return output.ToString();
    }

    [Benchmark]
    public string Format_Exception()
    {
        var output = new StringWriter();
        for (int i = 0; i < LogEvents; i++)
        {
            _formatter.Format(_logEventException, output);
        }
        return output.ToString();
    }
}