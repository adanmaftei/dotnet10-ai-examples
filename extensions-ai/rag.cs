// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.Extensions.AI@10.11.0
#:package OllamaSharp@5.5.0

using System.Numerics.Tensors;
using Microsoft.Extensions.AI;
using OllamaSharp;

var ollama = new Uri("http://localhost:11434");
IEmbeddingGenerator<string, Embedding<float>> embedder = new OllamaApiClient(ollama, "bge-m3");
IChatClient chat = new OllamaApiClient(ollama, "qwen2.5:7b-instruct");

string[] notes =
[
    "The office is closed on 1 December, Romania's national day.",
    "Expense reports must be submitted by the 5th of the following month.",
    "To get VPN access, open a ticket with IT.",
    "Parking spots are assigned by HR, first come, first served.",
    "Laptops are replaced every four years.",
];

// 1. Turn every note into a vector. A real app keeps these in a vector database.
var noteVectors = await embedder.GenerateAsync(notes);

// 2. Turn the question into a vector and keep the two closest notes.
var question = "By when do I have to hand in my expenses?";
var questionVector = await embedder.GenerateVectorAsync(question);

var closest = notes
    .Select((note, i) => (Note: note, Score: TensorPrimitives.CosineSimilarity(questionVector.Span, noteVectors[i].Vector.Span)))
    .OrderByDescending(match => match.Score)
    .Take(2)
    .ToList();

Console.WriteLine("Closest notes:");
foreach (var (note, score) in closest)
{
    Console.WriteLine($"  {score:F2}  {note}");
}

// 3. Let the model answer from those notes only.
List<ChatMessage> messages =
[
    new(ChatRole.System,
        "Answer only from these notes. If they don't contain the answer, say so.\n" +
        string.Join("\n", closest.Select(match => "- " + match.Note))),
    new(ChatRole.User, question),
];

var answer = await chat.GetResponseAsync(messages);
Console.WriteLine();
Console.WriteLine(answer.Text);
