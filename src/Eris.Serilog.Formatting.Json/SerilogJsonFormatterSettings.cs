namespace Eris.Serilog.Formatting.Json;

public sealed class SerilogJsonFormatterSettings
{
    public string TimestampName { get; init; } = "@timestamp";
    public string LogLevelName { get; init; } = "level";
    public string MessageName { get; init; } = "message";
    public string ExceptionName { get; init; } = "exception";
    public string TraceIdName { get; init; } = "traceId";
    public string SpanIdName { get; init; } = "spanId";
    public string PropertiesName { get; init; } = "properties";

    /// <summary>
    /// if true render message with properties, else use message template
    /// default: false
    /// </summary>
    public bool RenderMessage { get; init; }

    /// <summary>
    /// Wrap properties in a parent object use the name specified in <see cref="PropertiesName"/>
    /// default: false
    /// </summary>
    public bool WrapProperties { get; init; }

    /// <summary>
    /// Log Trace id
    /// </summary>
    public bool LogTraceId { get; init; } = true;

    /// <summary>
    /// Log Span id
    /// </summary>
    public bool LogSpanId { get; init; } = true;
}
