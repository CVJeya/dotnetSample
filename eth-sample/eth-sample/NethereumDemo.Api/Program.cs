using Microsoft.AspNetCore.Diagnostics;
using Nethereum.JsonRpc.Client;
using NethereumDemo.Api.Models;
using NethereumDemo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EthereumOptions>(builder.Configuration.GetSection(EthereumOptions.SectionName));
builder.Services.AddSingleton<IEthereumService, EthereumService>(); // one Web3 instance, one nonce tracker
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Map blockchain/validation failures to clean HTTP responses.
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, title) = ex switch
    {
        InvalidOperationException => (StatusCodes.Status400BadRequest, "Request not allowed"),
        RpcResponseException => (StatusCodes.Status502BadGateway, "Ethereum node returned an error"),
        HttpRequestException => (StatusCodes.Status502BadGateway, "Cannot reach the Ethereum node"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
    };

    ctx.Response.StatusCode = status;
    await ctx.Response.WriteAsJsonAsync(new { title, detail = ex?.Message });
}));

app.UseSwagger();
app.UseSwaggerUI();

// ------------------------------------------------------------------ endpoints

var eth = app.MapGroup("/api/eth").WithTags("Network");
eth.MapGet("/network", (IEthereumService s) => s.GetNetworkInfoAsync());
eth.MapGet("/balance/{address}", (string address, IEthereumService s) => s.GetBalanceAsync(address));

var board = app.MapGroup("/api/board").WithTags("MessageBoard contract");

// Reads: free, no signature needed (eth_call)
board.MapGet("/", (IEthereumService s) => s.GetBoardInfoAsync());
board.MapGet("/events", (ulong? fromBlock, IEthereumService s) => s.GetMessageUpdatedEventsAsync(fromBlock));

// Writes: signed transactions that cost gas
board.MapPost("/message", async (SetMessageRequest req, IEthereumService s) =>
{
    if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length > 280)
        return Results.BadRequest(new { title = "Message must be 1-280 characters." });

    return Results.Ok(await s.SetMessageAsync(req.Message));
});

board.MapPost("/deposit", async (DepositRequest req, IEthereumService s)
    => Results.Ok(await s.DepositAsync(req.AmountEth)));

app.Run();
