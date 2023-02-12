namespace Eris.Serilog.Formatting.Json.Tests;

[UsesVerify]
public sealed class SerilogJsonFormatterTests
{
    private static readonly DateTimeOffset _logEventDateTimeOffset = new(2023, 7, 15, 12, 50, 30, TimeSpan.FromHours(2));

    [Fact]
    public Task Text()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter();
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message", null, null);
    }

    [Fact]
    public Task Text_Use_Rendered_Message()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter(new() { RenderMessage = true });
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message", null, null);
    }

    [Fact]
    public Task Property()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter();
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message {Property}", null, new object[] { "value" });
    }

    [Fact]
    public Task Property_Use_Rendered_Message()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter(new() { RenderMessage = true });
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message {Property}", null, new object[] { "value" });
    }

    [Fact]
    public Task Property_Two()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter();
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message {Property}, {Property2}", null, new object[] { "value", "value2" });
    }

    [Fact]
    public Task Property_Object()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter();
        return LoggerVerify.VerifyLogAction(
            serilogJsonFormatter, 
            _logEventDateTimeOffset, 
            "Hello, {@User}, {N:x8} at {Now}", 
            null, 
            new object[] { new { Name = "nblumhardt", Tags = new[] { 1, 2, 3 } }, 123, _logEventDateTimeOffset });
    }

    [Fact]
    public Task Property_Format()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter();
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message {Property:X}", null, new object[] { 10 });
    }

    [Fact]
    public Task Exception()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter();
        return LoggerVerify.VerifyLogAction(
            serilogJsonFormatter,
            _logEventDateTimeOffset, 
            "message {Property}", 
            new NullReferenceException("exception message", new Exception("inner exception")), 
            new object[] { "value" });
    }

    [Fact]
    public Task Text_Wrap_Properties()
    {
        var serilogJsonFormatter = new SerilogJsonFormatter(new() { WrapProperties = true});
        return LoggerVerify.VerifyLogAction(serilogJsonFormatter, _logEventDateTimeOffset, "message {Property:X}, {Property2}", null, new object[] { 10, "two" });
    }
}
