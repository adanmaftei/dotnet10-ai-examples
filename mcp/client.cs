#:package ModelContextProtocol@2.3.0

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// The client starts server.cs as a child process and talks to it over stdin/stdout.
var folder = AppContext.GetData("EntryPointFileDirectoryPath") as string
    ?? throw new InvalidOperationException("Run this file with 'dotnet run client.cs'.");
var serverFile = Path.Combine(folder, "server.cs");

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "sample-server",
    Command = "dotnet",
    Arguments = ["run", serverFile],
});

await using var client = await McpClient.CreateAsync(transport);

Console.WriteLine("Tools the server offers:");
foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"  {tool.Name}: {tool.Description}");
}

Console.WriteLine();
await Call("echo", new() { ["message"] = "hello from the client" });
await Call("add", new() { ["a"] = 2, ["b"] = 40 });
await Call("utc_now", []);

async Task Call(string tool, Dictionary<string, object?> arguments)
{
    var result = await client.CallToolAsync(tool, arguments);
    var text = string.Join(" ", result.Content.OfType<TextContentBlock>().Select(block => block.Text));
    Console.WriteLine($"{tool} -> {text}");
}
