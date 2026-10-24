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
    .WithTools<SampleTools>()
    .WithResources<SampleResources>()
    .WithPrompts<SamplePrompts>();

await builder.Build().RunAsync();

// Tools are actions the model can run.

[McpServerToolType]
public sealed class SampleTools
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

// Resources are data the client can read, each one under a URI.
[McpServerResourceType]
public sealed class SampleResources
{
    [McpServerResource(UriTemplate = "notes://office-hours", Name = "office_hours", MimeType = "text/plain")]
    [Description("When the office is open.")]
    public static string OfficeHours() => "Monday to Friday, 9:00 to 18:00. Closed on public holidays.";
}

// Prompts are ready-made instructions the client can fill in and send to its model.
[McpServerPromptType]
public sealed class SamplePrompts
{
    [McpServerPrompt(Name = "summarize"), Description("Asks for a one-sentence summary of a text.")]
    public static string Summarize([Description("The text to summarize.")] string text) =>
        $"Summarize the following text in one sentence:\n\n{text}";
}
