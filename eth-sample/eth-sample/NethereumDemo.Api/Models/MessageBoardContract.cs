using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace NethereumDemo.Api.Models;

// These classes mirror the Solidity contract (contracts-hardhat/contracts/MessageBoard.sol).
// Nethereum uses the attributes to ABI-encode calls and decode results/events.

[Function("getMessage", "string")]
public class GetMessageFunction : FunctionMessage { }

[Function("setMessage")]
public class SetMessageFunction : FunctionMessage
{
    [Parameter("string", "newMessage", 1)]
    public string NewMessage { get; set; } = string.Empty;
}

// Payable function: the ETH to send is set through FunctionMessage.AmountToSend.
[Function("deposit")]
public class DepositFunction : FunctionMessage { }

[Function("owner", "address")]
public class OwnerFunction : FunctionMessage { }

[Function("updateCount", "uint256")]
public class UpdateCountFunction : FunctionMessage { }

[Function("contractBalance", "uint256")]
public class ContractBalanceFunction : FunctionMessage { }

[Event("MessageUpdated")]
public class MessageUpdatedEventDto : IEventDTO
{
    [Parameter("address", "author", 1, true)]
    public string Author { get; set; } = string.Empty;

    [Parameter("string", "newMessage", 2, false)]
    public string NewMessage { get; set; } = string.Empty;

    [Parameter("uint256", "updateCount", 3, false)]
    public BigInteger UpdateCount { get; set; }
}
