/*Learning path
#	Project	What you learn	Enterprise use
1	Chat with memory (done)	Messages list, system prompt, streaming	Basic assistant
2	Tool-calling agent (done, Program.cs)	Tools, agent loop, step limit	Foundation for everything below
3	Structured extraction: paste an email or ticket, get a C# record back (category, priority, customer, summary)	JSON schema output, validating model output, retrying on bad JSON	Ticket triage, invoice or form intake
4	Chat over your own documents: answer questions from a folder of SOPs or specs	Embeddings (nomic-embed-text in Ollama), similarity search, citing sources	Internal knowledge assistant
5	Error-triage agent: tools that read your DbErrorLogs table, then group failures and suggest likely causes	Read-only database tools, safe data access	Support and operations assistant
6	Agent that takes actions: creates a ticket or drafts an email, but asks you to approve first	Human-in-the-loop, separating “read” tools from “write” tools	Workflow automation
7	Two or more cooperating agents: one plans, one researches, one reviews	Orchestration, handing context between agents	Code review, report generation
8	Wrap it in ASP.NET Core: an API endpoint with dependency injection, logging and tests	Production structure	The real deployment

Stage 5 builds directly on the logging work from earlier in this conversation. You’d have real data and a real purpose for the agent.

Libraries to know
Microsoft.Extensions.AI (IChatClient): a common interface over any model. You code against it once, then switch between Ollama and Azure OpenAI by changing one registration line. This matters for enterprise code.
Semantic Kernel or Microsoft Agent Framework: they take over the loop, memory and multi-agent handling that you’ve written by hand. Learn the manual loop first, then use these. Check Microsoft’s current docs for which one they recommend now, since this area changes quickly.
Model Context Protocol (MCP): a standard way to expose your tools so other AI apps can call them.
Before using it in an enterprise application
Least privilege: give the agent read-only database access through a limited login or an allow-list of views, never your full connection string. For text-to-SQL, block everything except SELECT.
Keep critical logic in code: let the model decide which tool to call, but do money, tax and compliance calculations in normal C# functions. Never trust the model’s own arithmetic.
Approval for writes: anything that sends, updates or deletes should need a human click.
Audit trail: log each prompt, tool call and result to a table, the same way you log database failures. You’ll need it for debugging and compliance.
Limits: keep the max - steps cap, add timeouts, and track token usage and cost.
Prompt injection: treat text from emails, documents and web pages as untrusted. It can contain instructions that try to hijack the agent.
Data location: a local Ollama model keeps data on your machine. A cloud model sends it out, so check your company’s data policy before using real customer or tax data.
Testing: keep 20 to 30 sample inputs with expected outputs and re-run them after every prompt or model change. Agents break quietly.
Model choice

Ollama with small local models is for learning. They’re slower and less reliable at tool calling. For production, teams usually use Azure OpenAI or a similar hosted service, which is why the IChatClient abstraction pays off.

I’d start with stage 3. It’s small, quick to finish, and teaches you how much to trust model output. I can write the C# code for it, or for stage 5, if you want to continue.

RAG = Retrieval-Augmented Generation
The model answers using text it fetched at request time, instead of relying only on what it memorised during training.

Word	Meaning
Retrieval	Search an external corpus (docs, tickets, DB rows) for the top-k most relevant chunks to this specific question
Augmented	Those chunks are injected into the prompt as context
Generation	The LLM writes the answer conditioned on that context
The three problems it solves: the model doesn't know your private data, it doesn't know anything after its cutoff, and it can't cite sources. Note it changes what the model sees, not the model itself — no fine-tuning, no retraining, so a document updated 30 seconds ago can be in the answer.*/
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

using System.ClientModel;
using System.Data;
using System.Globalization;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

string endpoint = "https://local/openai";
string key = "key123";
string modelName = "phi";

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
