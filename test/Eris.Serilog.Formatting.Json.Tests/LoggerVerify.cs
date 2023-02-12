using Serilog;
using Serilog.Events;
using Serilog.Formatting;

namespace Eris.Serilog.Formatting.Json.Tests;

public static class LoggerVerify
{
    public static Task VerifyLogAction(ITextFormatter textFormatter, DateTimeOffset dateTimeOffset, string messageTemplate, Exception? exception, object?[] propertyValues)
    {
        using var output = new StringWriter();

        var logger = new LoggerConfiguration()
            .WriteTo.Sink(new TextWriterSink(output, textFormatter))
            .CreateLogger();

        if (logger.BindMessageTemplate(messageTemplate, propertyValues, out var parsedTemplate, out var boundProperties))
        {
            logger.Write(new LogEvent(dateTimeOffset, LogEventLevel.Information, exception, parsedTemplate, boundProperties));
        }

        // remove \r for running tests on windows with linux line endings
        return Verify(output.ToString().Replace("\\r", ""));
    }
}
