// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.Agents.AI@1.26.0
#:package OllamaSharp@5.5.0

using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:7b-instruct");

// An agent is a chat client plus instructions and tools.
AIAgent agent = chatClient.AsAIAgent(
    name: "OrderAgent",
    instructions: "You help customers with their orders. Keep answers to one sentence.",
    tools: [AIFunctionFactory.Create(GetOrderStatus, name: "get_order_status")]);

// A session keeps the conversation, so the second question can just say "it".
AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("> Where is order 1042?");
Console.WriteLine(await agent.RunAsync("Where is order 1042?", session));
Console.WriteLine("> When will it arrive?");
Console.WriteLine(await agent.RunAsync("When will it arrive?", session));

[Description("Looks up the delivery status of an order by its number.")]
static string GetOrderStatus([Description("The order number.")] int orderNumber)
{
    Console.WriteLine($"  (tool called: get_order_status({orderNumber}))");
    return orderNumber == 1042 ? "Shipped on 22 October, arriving on 25 October." : "No such order.";
}
