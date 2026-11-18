// Program.cs  -  a tool-using AI agent in C#
//
// Setup (normal .NET project, .NET 8 or later):
//     dotnet new console -n AgentDemo
//     cd AgentDemo
//     dotnet add package OpenAI
//     (replace Program.cs with this file, then: dotnet run)
//
// The three settings below match the Model Coder sample. In a real project,
// point them at Ollama (http://localhost:11434/v1), OpenAI, or an Azure OpenAI deployment.
//installed ollama locally using the below commands
//ollama --version
//ollama pull llama3.2
//ollama run llama3.2

using System.ClientModel;
using System.Data;
using System.Globalization;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

//string endpoint = "https://local/openai";
//string key = "key123";
//string modelName = "phi";

string endpoint = "http://localhost:11434/v1";
string key = "ollama";
string modelName = "llama3.2";


var client = new ChatClient(
    model: modelName,
    credential: new ApiKeyCredential(key),
    options: new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

// ---------------- 1. Tools: ordinary C# code ----------------
var notes = new List<string>();

string RunTool(string name, JsonElement args)
{
    switch (name)
    {
        case "calculate":
            string expr = args.GetProperty("expression").GetString()!;
            // DataTable.Compute handles + - * / and parentheses
            return Convert.ToString(new DataTable().Compute(expr, null), CultureInfo.InvariantCulture)!;
        case "get_time":
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        case "save_note":
            notes.Add(args.GetProperty("text").GetString()!);
            return "Note saved.";
        case "list_notes":
            return notes.Count == 0
                ? "No notes yet."
                : string.Join("\n", notes.Select((n, i) => $"{i + 1}. {n}"));
        default:
            return $"Unknown tool: {name}";
    }
}

// ---------------- 2. Tell the model what tools exist ----------------
// The description is what the model reads to decide when to call a tool.
var options = new ChatCompletionOptions
{
    Tools =
    {
        ChatTool.CreateFunctionTool(
            functionName: "calculate",
            functionDescription: "Evaluate an arithmetic expression, e.g. (12 + 8) * 3",
            functionParameters: BinaryData.FromString("""
                {"type":"object","properties":{"expression":{"type":"string"}},"required":["expression"]}
                """)),
        ChatTool.CreateFunctionTool(
            functionName: "get_time",
            functionDescription: "Get the current local date and time."),
        ChatTool.CreateFunctionTool(
            functionName: "save_note",
            functionDescription: "Save a short note for the user.",
            functionParameters: BinaryData.FromString("""
                {"type":"object","properties":{"text":{"type":"string"}},"required":["text"]}
                """)),
        ChatTool.CreateFunctionTool(
            functionName: "list_notes",
            functionDescription: "Return all saved notes."),
    }
};

// ---------------- 3. The agent loop ----------------
// ask model -> run requested tools -> feed results back -> repeat
var messages = new List<ChatMessage>
{
    new SystemChatMessage(
        "You are a concise, helpful assistant. Use the tools when they help. " +
        "Never guess arithmetic or the current time.")
};

string RunAgent(int maxSteps = 6)
{
    for (int step = 0; step < maxSteps; step++)
    {
        ChatCompletion completion = client.CompleteChat(messages, options);
        messages.Add(new AssistantChatMessage(completion));

        // No tool requested -> this is the final answer
        if (completion.FinishReason != ChatFinishReason.ToolCalls)
            return completion.Content.Count > 0 ? completion.Content[0].Text : "";

        foreach (ChatToolCall call in completion.ToolCalls)
        {
            string result;
            try
            {
                string json = call.FunctionArguments.ToString();
                using JsonDocument doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
                result = RunTool(call.FunctionName, doc.RootElement);
            }
            catch (Exception ex)
            {
                result = $"Error: {ex.Message}";   // let the model see the error and recover
            }

            Console.WriteLine($"  [tool] {call.FunctionName}({call.FunctionArguments}) -> {result}");
            messages.Add(new ToolChatMessage(call.Id, result));
        }
    }
    return "Stopped: too many steps without a final answer.";
}

Console.WriteLine("Agent ready. Try: What is 17.5% of 2480?  |  Save a note: call Priya at 5pm");
Console.WriteLine("Type 'quit' to exit.");

while (true)
{
    Console.Write("\nYou: ");
    string? input = Console.ReadLine();
    if (input is null || input.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase)) break;
    if (string.IsNullOrWhiteSpace(input)) continue;

    messages.Add(new UserChatMessage(input));
    try
    {
        Console.WriteLine("Agent: " + RunAgent());
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}
