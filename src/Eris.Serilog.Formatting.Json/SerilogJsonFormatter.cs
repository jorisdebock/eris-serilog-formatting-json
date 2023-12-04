using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Eris.Serilog.Formatting.Json;

public sealed class SerilogJsonFormatter : ITextFormatter
{
    private readonly StringBuilder _stringBuilder = new();
    private readonly SerilogJsonFormatterSettings _settings;
    private readonly JsonValueFormatter2 _jsonValueFormatter;

    public SerilogJsonFormatter(SerilogJsonFormatterSettings settings = null, JsonValueFormatter2 valueFormatter = null)
    {
        _settings = settings ?? new SerilogJsonFormatterSettings();
        _jsonValueFormatter = valueFormatter ?? new JsonValueFormatter2();
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        output.Write('{');

        WriteTimestamp(logEvent, output);
        output.Write(',');

        WriteLogLevel(logEvent, output);
        output.Write(',');

        WriteMessage(logEvent, output);
        WriteException(logEvent, output);
        WriteTraceId(logEvent, output);
        WriteSpanId(logEvent, output);
        WriteProperties(logEvent, output);

        output.Write('}');
        output.WriteLine();
    }

    private void WriteTimestamp(LogEvent logEvent, TextWriter output)
    {
        output.Write('"');
        output.Write(_settings.TimestampName);
        output.Write('"');
        output.Write(':');
        _jsonValueFormatter.WriteQuotedJsonString(logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture), output);
    }

    private void WriteLogLevel(LogEvent logEvent, TextWriter output)
    {
        output.Write('"');
        output.Write(_settings.LogLevelName);
        output.Write('"');
        output.Write(':');
        _jsonValueFormatter.WriteQuotedJsonString(GetLogLevelString(logEvent.Level), output);
    }

    private string GetLogLevelString(LogEventLevel logEventLevel)
    {
        return logEventLevel switch
        {
            LogEventLevel.Verbose => nameof(LogEventLevel.Verbose),
            LogEventLevel.Debug => nameof(LogEventLevel.Debug),
            LogEventLevel.Information => nameof(LogEventLevel.Information),
            LogEventLevel.Warning => nameof(LogEventLevel.Warning),
            LogEventLevel.Error => nameof(LogEventLevel.Error),
            LogEventLevel.Fatal => nameof(LogEventLevel.Fatal),
            _ => string.Empty
        };
    }

    private void WriteMessage(LogEvent logEvent, TextWriter output)
    {
        output.Write('"');
        output.Write(_settings.MessageName);
        output.Write('"');
        output.Write(':');

        if (_settings.RenderMessage)
        {
            _stringBuilder.Clear();
            var space = new StringWriter(_stringBuilder);
            logEvent.RenderMessage(space);
            _jsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
        }
        else
        {
            _jsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);
        }
    }

    private void WriteException(LogEvent logEvent, TextWriter output)
    {
        if (logEvent.Exception == null)
        {
            return;
        }

        output.Write(',');

        output.Write('"');
        output.Write(_settings.ExceptionName);
        output.Write('"');
        output.Write(':');
        _jsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
    }

    private void WriteTraceId(LogEvent logEvent, TextWriter output)
    {
        if (!logEvent.TraceId.HasValue || !_settings.LogTraceId)
        {
            return;
        }

        output.Write(',');

        output.Write('"');
        output.Write(_settings.TraceIdName);
        output.Write('"');
        output.Write(':');
        _jsonValueFormatter.WriteQuotedJsonString(logEvent.TraceId.Value.ToHexString(), output);
    }

    private void WriteSpanId(LogEvent logEvent, TextWriter output)
    {
        if (!logEvent.SpanId.HasValue || !_settings.LogSpanId)
        {
            return;
        }

        output.Write(',');

        output.Write('"');
        output.Write(_settings.SpanIdName);
        output.Write('"');
        output.Write(':');
        _jsonValueFormatter.WriteQuotedJsonString(logEvent.SpanId.Value.ToHexString(), output);
    }

    private void WriteProperties(LogEvent logEvent, TextWriter output)
    {
        var tokens = logEvent.MessageTemplate.Tokens is MessageTemplateToken[] tokensArray
            ? tokensArray
            : logEvent.MessageTemplate.Tokens.ToArray();

        bool skipComma = false;
        if (_settings.WrapProperties && logEvent.Properties.Count > 0)
        {
            output.Write(',');
            output.Write('"');
            output.Write(_settings.PropertiesName);
            output.Write('"');
            output.Write(':');

            output.Write('{');

            skipComma = true;
        }

        foreach (var property in logEvent.Properties)
        {
            if (!skipComma)
            {
                output.Write(',');
            }
            skipComma = false;

            _jsonValueFormatter.WriteQuotedJsonString(property.Key, output, camelCase: true);
            output.Write(':');

            var propertyToken = GetPropertyToken(tokens, property.Key);
            if (propertyToken != null)
            {
                _stringBuilder.Clear();
                var space = new StringWriter(_stringBuilder);
                propertyToken.Render(logEvent.Properties, space);
                _jsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
            }
            else
            {
                _jsonValueFormatter.Format(property.Value, output);
            }
        }

        if (_settings.WrapProperties && logEvent.Properties.Count > 0)
        {
            output.Write('}');
        }
    }

    private PropertyToken GetPropertyToken(MessageTemplateToken[] tokens, string name)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            if (token is not PropertyToken propertyToken)
            {
                continue;
            }
            if (propertyToken.Format is null)
            {
                continue;
            }
            if (propertyToken.PropertyName == name)
            {
                return propertyToken;
            }
        }
        return null;
    }
}