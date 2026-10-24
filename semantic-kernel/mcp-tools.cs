// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.SemanticKernel@1.81.0
#:package Microsoft.SemanticKernel.Connectors.Ollama@1.81.0-alpha
#:package ModelContextProtocol@2.3.0

using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

#pragma warning disable SKEXP0001, SKEXP0070 // AsKernelFunction and the Ollama connector are still marked experimental.

var folder = AppContext.GetData("EntryPointFileDirectoryPath") as string
    ?? throw new InvalidOperationException("Run this file with 'dotnet run mcp-tools.cs'.");

await using var mcp = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "sample-server",
    Command = "dotnet",
    Arguments = ["run", Path.Combine(folder, "..", "mcp", "server.cs")],
}));

var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion("qwen2.5:7b-instruct", new Uri("http://localhost:11434"));
var kernel = builder.Build();

// The MCP tools become a normal kernel plugin.
var tools = await mcp.ListToolsAsync();
kernel.Plugins.AddFromFunctions("mcp", tools.Select(tool => tool.AsKernelFunction()));

var settings = new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
var answer = await kernel.InvokePromptAsync("What is the current UTC date and time?", new KernelArguments(settings));
Console.WriteLine(answer);
