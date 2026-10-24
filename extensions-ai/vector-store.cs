// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package CommunityToolkit.VectorData.InMemory@1.0.1
#:package OllamaSharp@5.5.0

using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OllamaSharp;

IEmbeddingGenerator<string, Embedding<float>> embedder = new OllamaApiClient(new Uri("http://localhost:11434"), "bge-m3");

// The store calls the embedding generator itself, on insert and on search.
// Swap InMemoryVectorStore for Qdrant, PostgreSQL, Azure AI Search, ... and the rest stays the same.
var store = new InMemoryVectorStore(new InMemoryVectorStoreOptions { EmbeddingGenerator = embedder });
var collection = store.GetCollection<int, Note>("notes");
await collection.EnsureCollectionExistsAsync();

await collection.UpsertAsync(
[
    new Note { Id = 1, Text = "The office is closed on 1 December, Romania's national day." },
    new Note { Id = 2, Text = "Expense reports must be submitted by the 5th of the following month." },
    new Note { Id = 3, Text = "To get VPN access, open a ticket with IT." },
    new Note { Id = 4, Text = "Parking spots are assigned by HR, first come, first served." },
    new Note { Id = 5, Text = "Laptops are replaced every four years." },
]);

var question = "By when do I have to hand in my expenses?";
Console.WriteLine(question);
await foreach (var match in collection.SearchAsync(question, top: 2))
{
    Console.WriteLine($"  {match.Score:F2}  {match.Record.Text}");
}

public sealed class Note
{
    [VectorStoreKey]
    public int Id { get; set; }

    [VectorStoreData]
    public required string Text { get; set; }

    // bge-m3 produces 1024 numbers per text. The store embeds Text to fill this.
    [VectorStoreVector(1024)]
    public string Embedding => Text;
}
