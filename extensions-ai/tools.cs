// File-based apps default to Native AOT, which turns off the reflection-based JSON that OllamaSharp uses.
#:property PublishAot=false
#:package Microsoft.Extensions.AI@10.11.0
#:package OllamaSharp@5.5.0

using System.ComponentModel;
using Microsoft.Extensions.AI;
using OllamaSharp;

// UseFunctionInvocation runs the tool calls the model asks for and sends the results back to it.
IChatClient client = new ChatClientBuilder(new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:7b-instruct"))
    .UseFunctionInvocation()
    .Build();

var options = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(GetOrderStatus, name: "get_order_status")],
};

var response = await client.GetResponseAsync("What is the status of order 1042?", options);
Console.WriteLine(response.Text);

[Description("Looks up the delivery status of an order by its number.")]
static string GetOrderStatus([Description("The order number.")] int orderNumber)
{
    Console.WriteLine($"  (tool called: get_order_status({orderNumber}))");
    return orderNumber == 1042 ? "Shipped on 22 October, arriving on 25 October." : "No such order.";
}
