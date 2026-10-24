#:package ModelContextProtocol@2.3.0

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// Start http-server.cs first. Over HTTP the server is a separate, already running process.
await using var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri("http://127.0.0.1:3001/mcp"),
}));

foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"{tool.Name}: {tool.Description}");
}

var result = await client.CallToolAsync("utc_now");
Console.WriteLine($"utc_now -> {result.Content.OfType<TextContentBlock>().Single().Text}");
