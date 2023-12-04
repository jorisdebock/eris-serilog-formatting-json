using System.Diagnostics;

using Eris.Serilog.Formatting.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var user = new User("Foo", 22, new[] { 1, 2, 3 }, 0xffffff);

Console.WriteLine("SerilogJsonFormatter default settings:");
{
    var logger = new LoggerConfiguration()
        .WriteTo.Console(new SerilogJsonFormatter())
        .CreateLogger();

    logger.Information("Hello, Foo");
    logger.Information("Hello, {Name}", user.Name);
    logger.Information("Hello, {Name}, {Age}", user.Name, user.Age);
    logger.Information("Hello, {@User}", user);
    logger.Information("Color: {Color:x}", user.Color);
    logger.Error(new Exception("exception message", new Exception("inner exception message")), "error");
}
Console.WriteLine();
Console.WriteLine("SerilogJsonFormatter wrap properties");
{
    var logger = new LoggerConfiguration()
        .WriteTo.Console(new SerilogJsonFormatter(new() { WrapProperties = true }))
        .CreateLogger();

    logger.Information("Hello, {Name}", user.Name);
    logger.Information("Hello, {@User}", user);
}
Console.WriteLine();
Console.WriteLine("SerilogJsonFormatter render message");
{
    var logger = new LoggerConfiguration()
        .WriteTo.Console(new SerilogJsonFormatter(new() { RenderMessage = true }))
        .CreateLogger();

    logger.Information("Hello, {Name}", user.Name);
    logger.Information("Hello, {Name}, {Age}", user.Name, user.Age);
    logger.Information("Hello, {@User}", user);
}
Console.WriteLine();
Console.WriteLine("SerilogJsonFormatter log Activity traceId and spanId");
{
    var logger = new LoggerConfiguration()
        .WriteTo.Console(new SerilogJsonFormatter(new() { RenderMessage = true, LogTraceId = true, LogSpanId = true }))
        .CreateLogger();

    Activity.Current = new Activity("test").Start();
    logger.Information("Hello, {Name}", user.Name);
    Activity.Current.Stop();
    Activity.Current = null;
}
Console.WriteLine();
Console.WriteLine("SerilogJsonFormatter default settings using Microsoft.Extensions.Logging.Ilogger");
{
    var logger = new LoggerConfiguration()
        .WriteTo.Console(new SerilogJsonFormatter())
        .CreateLogger();

    var builder = Host.CreateDefaultBuilder(args);
    builder.UseSerilog(logger);
    var app = builder.Build();

    var mlogger = app.Services.GetRequiredService<ILogger<User>>();

    mlogger.LogInformation("Hello, Foo");
    mlogger.LogInformation("Hello, {Name}", user.Name);
    mlogger.LogInformation("Hello, {@User}", user);
    mlogger.LogInformation("Color: {Color:x}", user.Color);
    mlogger.LogError(new Exception("exception message", new Exception("inner exception message")), "error");

    using (mlogger.BeginScope(new Dictionary<string, object> { { "userId", "123" } }))
    {
        mlogger.LogInformation("Hello, {Name}", user.Name);
    }
}

public sealed record User(string Name, int Age, int[] Tags, int Color);