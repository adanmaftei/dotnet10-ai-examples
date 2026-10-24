// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package OllamaSharp@5.5.0

using Microsoft.Extensions.AI;
using OllamaSharp;

// IChatClient is the common interface. Only this line knows the model runs in Ollama.
IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:7b-instruct");

// One question, one answer.
var response = await client.GetResponseAsync("In one sentence, what does the await keyword do in C#?");
Console.WriteLine(response.Text);
Console.WriteLine();

// A short conversation, printed as the tokens arrive.
List<ChatMessage> conversation =
[
    new(ChatRole.System, "Answer in at most two sentences."),
    new(ChatRole.User, "Name one advantage of dependency injection in .NET."),
];

await foreach (var update in client.GetStreamingResponseAsync(conversation))
{
    Console.Write(update.Text);
}

Console.WriteLine();
