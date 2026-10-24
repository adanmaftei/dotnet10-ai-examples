# .NET 10 AI examples: MCP, Microsoft.Extensions.AI, Semantic Kernel

Seven small, working examples. Each one is a single `.cs` file you run with `dotnet run`, with
no project file. That's the .NET 10 "file-based app" feature: the `#:package` lines at the top
of each file pull in the NuGet packages.

| File | What it shows |
|---|---|
| `mcp/server.cs` | An MCP server with three tools (`echo`, `add`, `utc_now`), talking over stdin/stdout |
| `mcp/client.cs` | An MCP client that starts that server, lists its tools and calls each one. No AI model involved |
| `extensions-ai/chat.cs` | `IChatClient`: one question, then a short conversation streamed token by token |
| `extensions-ai/tools.cs` | The model calls a plain C# method as a tool |
| `extensions-ai/mcp-tools.cs` | The model calls the tools of the MCP server above |
| `semantic-kernel/plugin.cs` | The same order-status example as `tools.cs`, written as a Semantic Kernel plugin |
| `semantic-kernel/mcp-tools.cs` | The MCP server's tools turned into a Semantic Kernel plugin |

## What you need

- .NET 10 SDK.
- For everything except the `mcp` folder: [Ollama](https://ollama.com) on `localhost:11434`
  with a model that supports tool calling. The examples use `qwen2.5:7b-instruct`:

  ```bash
  ollama pull qwen2.5:7b-instruct
  ```

  Ollama in Docker works too, as long as port 11434 is published.

## Run

Run each file from its own folder:

```bash
cd mcp
dotnet run client.cs

cd ../extensions-ai
dotnet run chat.cs
dotnet run tools.cs
dotnet run mcp-tools.cs

cd ../semantic-kernel
dotnet run plugin.cs
dotnet run mcp-tools.cs
```

The first run of each file restores packages and builds, which takes a little while. After that
they start in a few seconds.

What I got (Windows 11, .NET SDK 10.0.401, Ollama 0.24.0 in Docker, `qwen2.5:7b-instruct`).
Model answers vary from run to run:

```text
mcp/client.cs
  echo -> You said: hello from the client
  add -> 42
  utc_now -> 2026-10-24T11:15:59.6290651+00:00

extensions-ai/tools.cs
  (tool called: get_order_status(1042))
  The order with number 1042 was shipped on October 22 and is expected to arrive on October 25.

extensions-ai/mcp-tools.cs
  (model called the MCP tool 'add')
  The sum of 1234 and 5678 is 6912.

semantic-kernel/mcp-tools.cs
  The current UTC date and time is 2026-10-24T11:15:56.2907824Z.
```

## How the pieces fit

**MCP (Model Context Protocol)** is a standard way for an AI app to discover and call tools that
live in another process. The server in `mcp/server.cs` is short: mark a static class with
`[McpServerToolType]`, mark methods with `[McpServerTool]`, and
`AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()` does the rest. Any MCP host
that can start a stdio server can use it: the command is `dotnet` and the arguments are
`run /full/path/to/mcp/server.cs`.

**Microsoft.Extensions.AI** gives you one interface, `IChatClient`, for any model provider.
`ChatClientBuilder(...).UseFunctionInvocation()` adds the loop that runs the tools the model asks
for and sends back the results. Tools are `AIFunction`s. An MCP client tool is already an
`AIFunction`, so the list from `ListToolsAsync()` goes straight into `ChatOptions.Tools`.

**Semantic Kernel** sits on top: a `Kernel` holds the AI service and your plugins, and
`FunctionChoiceBehavior.Auto()` lets the model call plugin functions. `AsKernelFunction()` turns
an MCP tool into a kernel function. Microsoft calls
[Agent Framework](https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-and-microsoft-agent-framework/)
the successor to Semantic Kernel, so new features land there, while Semantic Kernel 1.x still gets
critical fixes.

## Things worth knowing

- **Switching providers.** Only the line that creates the client knows about Ollama. Swap it for
  OpenAI, Azure OpenAI or another `IChatClient` and the rest stays the same.
- **`#:property PublishAot=false`.** File-based apps default to Native AOT, which turns off
  reflection-based JSON. OllamaSharp uses it, so without this line every call fails with
  "Reflection-based serialization has been disabled". The MCP files don't need it.
- **Experimental APIs.** Semantic Kernel's Ollama connector (`1.81.0-alpha`) and
  `AsKernelFunction()` are marked experimental, so those files suppress `SKEXP0070` and
  `SKEXP0001`.
- **Logs go to stderr in the MCP server.** stdout carries the protocol messages, and a stray log
  line there would break the connection.

## Versions

ModelContextProtocol 2.3.0, Microsoft.Extensions.AI 10.11.0, OllamaSharp 5.5.0,
Microsoft.SemanticKernel 1.81.0, Microsoft.Extensions.Hosting 10.0.12.
