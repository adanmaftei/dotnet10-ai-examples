// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.Extensions.AI@10.11.0
#:package Microsoft.Extensions.Caching.Memory@10.0.13
#:package OllamaSharp@5.5.0

using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OllamaSharp;

IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

// Each step wraps the next one: timing, then the cache, then the model.
IChatClient client = new ChatClientBuilder(new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:7b-instruct"))
    .Use(getResponseFunc: TimeIt, getStreamingResponseFunc: null)
    .UseDistributedCache(cache)
    .Build();

// The second identical question is answered from the cache, without calling the model.
for (var attempt = 1; attempt <= 2; attempt++)
{
    var response = await client.GetResponseAsync("Name three prime numbers.");
    Console.WriteLine($"  {response.Text}");
}

// A middleware is a function that gets the request and the next client in the chain.
static async Task<ChatResponse> TimeIt(
    IEnumerable<ChatMessage> messages, ChatOptions? options, IChatClient next, CancellationToken cancellationToken)
{
    var watch = Stopwatch.StartNew();
    var response = await next.GetResponseAsync(messages, options, cancellationToken);
    Console.WriteLine($"call took {watch.ElapsedMilliseconds} ms");
    return response;
}
