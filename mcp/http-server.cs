#:sdk Microsoft.NET.Sdk.Web
#:package ModelContextProtocol.AspNetCore@2.3.0

using System.ComponentModel;
using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<ClockTools>();

var app = builder.Build();
app.MapMcp("/mcp");

// The MCP spec asks local servers to listen on localhost only, not on every network interface.
app.Run("http://127.0.0.1:3001");

[McpServerToolType]
public sealed class ClockTools
{
    [McpServerTool(Name = "utc_now"), Description("Returns the current UTC date and time in ISO 8601 format.")]
    public static string UtcNow() => DateTimeOffset.UtcNow.ToString("O");
}
