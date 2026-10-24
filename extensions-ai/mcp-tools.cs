// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.Extensions.AI@10.11.0
#:package OllamaSharp@5.5.0
#:package ModelContextProtocol@2.3.0

using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

var folder = AppContext.GetData("EntryPointFileDirectoryPath") as string
    ?? throw new InvalidOperationException("Run this file with 'dotnet run mcp-tools.cs'.");

await using var mcp = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "sample-server",
    Command = "dotnet",
    Arguments = ["run", Path.Combine(folder, "..", "mcp", "server.cs")],
}));

// Every MCP tool is already an AIFunction, so the list goes straight into ChatOptions.
var tools = await mcp.ListToolsAsync();

IChatClient client = new ChatClientBuilder(new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:7b-instruct"))
    .UseFunctionInvocation()
    .Build();

var response = await client.GetResponseAsync("What is 1234 plus 5678?", new ChatOptions { Tools = [.. tools] });

foreach (var call in response.Messages.SelectMany(message => message.Contents).OfType<FunctionCallContent>())
{
    Console.WriteLine($"  (model called the MCP tool '{call.Name}')");
}

Console.WriteLine(response.Text);
