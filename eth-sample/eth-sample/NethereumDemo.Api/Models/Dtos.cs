namespace NethereumDemo.Api.Models;

// Wei amounts are returned as strings: they can exceed what JavaScript numbers can hold exactly.

public record SetMessageRequest(string Message);

public record DepositRequest(decimal AmountEth);

public record NetworkInfo(long ConfiguredChainId, long NodeChainId, string BlockNumber, string GasPriceWei, bool CanSign, string? SignerAddress);

public record BalanceResult(string Address, string BalanceWei, decimal BalanceEth);

public record BoardInfo(string ContractAddress, string Owner, string Message, string UpdateCount, string ContractBalanceWei, decimal ContractBalanceEth);

public record TxResult(string TransactionHash, string BlockNumber, bool Success, string GasUsed);

public record MessageUpdatedItem(string Author, string Message, string UpdateCount, string BlockNumber, string TransactionHash);
