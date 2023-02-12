namespace Eris.Serilog.Formatting.Json;

public sealed class SerilogJsonFormatterSettings
{
    public string TimestampName { get; init; } = "@timestamp";
    public string LogLevelName { get; init; } = "level";
    public string MessageName { get; init; } = "message";
    public string ExceptionName { get; init; } = "exception";
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
}
