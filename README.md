# .NET 10 AI examples: MCP, Microsoft.Extensions.AI, Semantic Kernel, Agent Framework

Twelve small, working examples. Each one is a single `.cs` file you run with `dotnet run`, with
no project file. That's the .NET 10 "file-based app" feature: the `#:package` lines at the top
of each file pull in the NuGet packages.

| File | What it shows |
|---|---|
| `mcp/server.cs` | An MCP server with three tools (`echo`, `add`, `utc_now`), talking over stdin/stdout |
| `mcp/client.cs` | An MCP client that starts that server, lists its tools and calls each one. No AI model involved |
| `mcp/http-server.cs` | The same idea over HTTP: an ASP.NET Core MCP server on `127.0.0.1:3001/mcp` |
| `mcp/http-client.cs` | A client that connects to the running HTTP server |
| `extensions-ai/chat.cs` | `IChatClient`: one question, then a short conversation streamed token by token |
| `extensions-ai/tools.cs` | The model calls a plain C# method as a tool |
| `extensions-ai/mcp-tools.cs` | The model calls the tools of the stdio MCP server above |
| `extensions-ai/structured-output.cs` | The model fills in a C# record (`GetResponseAsync<T>`) |
| `extensions-ai/rag.cs` | Retrieval-augmented generation: embeddings, cosine similarity, an answer from the closest notes |
| `semantic-kernel/plugin.cs` | The same order-status example as `tools.cs`, written as a Semantic Kernel plugin |
| `semantic-kernel/mcp-tools.cs` | The MCP server's tools turned into a Semantic Kernel plugin |
| `agent-framework/agent.cs` | An agent with instructions, a tool and a session that remembers the conversation |

## What you need

- .NET 10 SDK.
- For everything except the `mcp` folder: [Ollama](https://ollama.com) on `localhost:11434` with
  a chat model that supports tool calling and, for `rag.cs`, an embedding model:

  ```bash
  ollama pull qwen2.5:7b-instruct
  ollama pull bge-m3
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
dotnet run structured-output.cs
dotnet run rag.cs

cd ../semantic-kernel
dotnet run plugin.cs
dotnet run mcp-tools.cs

cd ../agent-framework
dotnet run agent.cs
```

The HTTP pair needs two terminals, because the server keeps running:

```bash
cd mcp
dotnet run http-server.cs   # terminal 1
dotnet run http-client.cs   # terminal 2
```

The first run of each file restores packages and builds, which takes a little while. After that
they start in a few seconds.

What I got (Windows 11, .NET SDK 10.0.401, Ollama 0.24.0 in Docker, `qwen2.5:7b-instruct` and
`bge-m3`). Model answers vary from run to run:

```text
mcp/client.cs
  echo -> You said: hello from the client
  add -> 42
  utc_now -> 2026-10-24T11:15:59.6290651+00:00

extensions-ai/tools.cs
  (tool called: get_order_status(1042))
  The order with number 1042 was shipped on October 22 and is expected to arrive on October 25.

extensions-ai/structured-output.cs
  Number:   2026-118
  Supplier: Contoso SRL
  Total:    1250.50 EUR
  Due:      15 November 2026

extensions-ai/rag.cs
  Closest notes:
    0.74  Expense reports must be submitted by the 5th of the following month.
    0.45  Laptops are replaced every four years.

  You have to hand in your expenses by the 5th of the following month.

semantic-kernel/mcp-tools.cs
  The current UTC date and time is 2026-10-24T11:15:56.2907824Z.

agent-framework/agent.cs
  > Where is order 1042?
    (tool called: get_order_status(1042))
  Your order 1042 was shipped on 22 October and is expected to arrive by 25 October.
  > When will it arrive?
  Your order 1042 is expected to arrive by 25 October.
```

## How the pieces fit

**MCP (Model Context Protocol)** is a standard way for an AI app to discover and call tools that
live somewhere else. The server in `mcp/server.cs` is short: mark a class with
`[McpServerToolType]`, mark static methods with `[McpServerTool]`, and
`AddMcpServer().WithStdioServerTransport().WithTools<SampleTools>()` does the rest. Any MCP
host that can start a stdio server can use it: the command is `dotnet` and the arguments are
`run /full/path/to/mcp/server.cs`. Over HTTP, the server is a normal ASP.NET Core app:
`WithHttpTransport()` plus `app.MapMcp("/mcp")`, and clients connect with `HttpClientTransport`.

**Microsoft.Extensions.AI** gives you one interface, `IChatClient`, for any model provider.
`ChatClientBuilder(...).UseFunctionInvocation()` adds the loop that runs the tools the model asks
for and sends back the results. Tools are `AIFunction`s. An MCP client tool is already an
`AIFunction`, so the list from `ListToolsAsync()` goes straight into `ChatOptions.Tools`.
`GetResponseAsync<T>()` asks for JSON that matches `T` and parses it. `IEmbeddingGenerator`
turns text into vectors, which is the base of retrieval-augmented generation.

**Semantic Kernel** sits on top: a `Kernel` holds the AI service and your plugins, and
`FunctionChoiceBehavior.Auto()` lets the model call plugin functions. `AsKernelFunction()` turns
an MCP tool into a kernel function.

**Agent Framework** is what Microsoft calls the
[successor to Semantic Kernel](https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-and-microsoft-agent-framework/).
New features land there, while Semantic Kernel 1.x still gets critical fixes. An agent is an
`IChatClient` plus instructions and tools (`AsAIAgent(...)`). An `AgentSession` keeps the
conversation between calls.

## Things worth knowing

- **Switching providers.** Only the line that creates the client knows about Ollama. Swap it for
  OpenAI, Azure OpenAI or another `IChatClient` and the rest stays the same.
- **`#:property PublishAot=false`.** File-based apps default to Native AOT, which turns off
  reflection-based JSON. OllamaSharp uses it, so without this line every call fails with
  "Reflection-based serialization has been disabled". The MCP files don't need it.
- **`WithTools<T>()`, not `WithToolsFromAssembly()`.** Under that same AOT default,
  `WithToolsFromAssembly()` makes the first build print warning IL2026, and with `dotnet run` that
  warning lands on the server's stdout. The MCP spec says a stdio server
  [must not write anything to stdout that isn't an MCP message](https://modelcontextprotocol.io/specification/2025-06-18/basic/transports#stdio),
  which is also why the server sends its logs to stderr. The C# client in this repo happened to
  skip such lines when I tried it, but other hosts don't have to.
- **Small models and typed dates.** In `structured-output.cs` the due date is a `string`. As a
  `DateOnly`, `qwen2.5:7b-instruct` wrote a wrong date (for example `1511-11-20` for
  "15 November 2026") in 11 of 12 test runs, with or without a format hint in a `[Description]`.
  As text it was right in all 8 runs.
- **The model doesn't know what it wasn't trained on.** `chat.cs` first asked the model what MCP
  is, and it answered confidently and wrongly. RAG (`rag.cs`) and tools are how you give a model
  facts it doesn't have.
- **Experimental APIs.** Semantic Kernel's Ollama connector (`1.81.0-alpha`) and
  `AsKernelFunction()` are marked experimental, so those files suppress `SKEXP0070` and
  `SKEXP0001`.

## Versions

ModelContextProtocol 2.3.0, ModelContextProtocol.AspNetCore 2.3.0, Microsoft.Extensions.AI 10.11.0,
OllamaSharp 5.5.0, Microsoft.SemanticKernel 1.81.0, Microsoft.Agents.AI 1.26.0,
Microsoft.Extensions.Hosting 10.0.12.
