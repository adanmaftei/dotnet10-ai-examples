// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.SemanticKernel@1.81.0
#:package Microsoft.SemanticKernel.Connectors.Ollama@1.81.0-alpha

using System.ComponentModel;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070 // The Ollama connector is still marked experimental.

var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion("qwen2.5:7b-instruct", new Uri("http://localhost:11434"));
builder.Plugins.AddFromType<OrderPlugin>("orders");
var kernel = builder.Build();

// Auto lets the model decide when to call a plugin function; the kernel runs it and returns the result.
var settings = new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
var answer = await kernel.InvokePromptAsync("What is the status of order 1042?", new KernelArguments(settings));
Console.WriteLine(answer);

public sealed class OrderPlugin
{
    [KernelFunction("get_order_status"), Description("Looks up the delivery status of an order by its number.")]
    public string GetOrderStatus([Description("The order number.")] int orderNumber)
    {
        Console.WriteLine($"  (plugin called: get_order_status({orderNumber}))");
        return orderNumber == 1042 ? "Shipped on 22 October, arriving on 25 October." : "No such order.";
    }
}
