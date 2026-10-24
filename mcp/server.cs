#:package ModelContextProtocol@2.3.0
#:package Microsoft.Extensions.Hosting@10.0.12

using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP messages, so every log line has to go to stderr.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class SampleTools
{
    [McpServerTool(Name = "echo"), Description("Returns the message you send.")]
    public static string Echo([Description("Any text.")] string message) => $"You said: {message}";

    [McpServerTool(Name = "add"), Description("Adds two whole numbers.")]
    public static long Add(
        [Description("First number.")] long a,
        [Description("Second number.")] long b) => a + b;

    [McpServerTool(Name = "utc_now"), Description("Returns the current UTC date and time in ISO 8601 format.")]
    public static string UtcNow() => DateTimeOffset.UtcNow.ToString("O");
}
