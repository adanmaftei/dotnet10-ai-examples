// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.Agents.AI@1.26.0
#:package Microsoft.Agents.AI.Workflows@1.26.0
#:package OllamaSharp@5.5.0

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:7b-instruct");

AIAgent writer = chatClient.AsAIAgent(
    name: "Writer",
    instructions: "Write a two-sentence product description in English for what the user names.");

AIAgent reviewer = chatClient.AsAIAgent(
    name: "Reviewer",
    instructions: "Rewrite the previous product description in English: make it shorter and remove any exaggeration. Reply with the new text only.");

// In a sequential workflow each agent sees the conversation so far, including the previous agent's answer.
var workflow = AgentWorkflowBuilder.BuildSequential([writer, reviewer]);

// A workflow can run as an agent itself.
AIAgent team = workflow.AsAIAgent();
var response = await team.RunAsync("A reusable steel water bottle.");

foreach (var message in response.Messages)
{
    Console.WriteLine($"[{message.AuthorName}] {message.Text}");
    Console.WriteLine();
}
