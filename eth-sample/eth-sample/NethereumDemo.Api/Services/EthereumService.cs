using System.Numerics;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NethereumDemo.Api.Models;

namespace NethereumDemo.Api.Services;

public interface IEthereumService
{
    Task<NetworkInfo> GetNetworkInfoAsync();
    Task<BalanceResult> GetBalanceAsync(string address);
    Task<BoardInfo> GetBoardInfoAsync();
    Task<TxResult> SetMessageAsync(string message);
    Task<TxResult> DepositAsync(decimal amountEth);
    Task<IReadOnlyList<MessageUpdatedItem>> GetMessageUpdatedEventsAsync(ulong? fromBlock);
}

public class EthereumService : IEthereumService
{
    private readonly EthereumOptions _options;
    private readonly Web3 _web3;
    private readonly Account? _account;
    private bool _chainVerified;

    public EthereumService(IOptions<EthereumOptions> options)
    {
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.RpcUrl))
            throw new InvalidOperationException("Ethereum:RpcUrl is not configured.");

        if (!string.IsNullOrWhiteSpace(_options.PrivateKey))
        {
            // Passing the chain id makes Nethereum sign EIP-155 / EIP-1559 transactions for that chain only.
            _account = new Account(_options.PrivateKey, new BigInteger(_options.ChainId));
            _web3 = new Web3(_account, _options.RpcUrl);
        }
        else
        {
            _web3 = new Web3(_options.RpcUrl); // read-only
        }
    }

    // ---------------------------------------------------------------- network

    public async Task<NetworkInfo> GetNetworkInfoAsync()
    {
        var chainId = await _web3.Eth.ChainId.SendRequestAsync();
        var block = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

        return new NetworkInfo(
            _options.ChainId,
            (long)chainId.Value,
            block.Value.ToString(),
            gasPrice.Value.ToString(),
            _account is not null,
            _account?.Address);
    }

    public async Task<BalanceResult> GetBalanceAsync(string address)
    {
        EnsureValidAddress(address);
        var wei = (await _web3.Eth.GetBalance.SendRequestAsync(address)).Value;
        return new BalanceResult(address, wei.ToString(), Web3.Convert.FromWei(wei));
    }

    // --------------------------------------------------------------- contract

    public async Task<BoardInfo> GetBoardInfoAsync()
    {
        var contract = RequireContractAddress();

        var message = await _web3.Eth.GetContractQueryHandler<GetMessageFunction>()
            .QueryAsync<string>(contract, new GetMessageFunction());
        var owner = await _web3.Eth.GetContractQueryHandler<OwnerFunction>()
            .QueryAsync<string>(contract, new OwnerFunction());
        var updates = await _web3.Eth.GetContractQueryHandler<UpdateCountFunction>()
            .QueryAsync<BigInteger>(contract, new UpdateCountFunction());
        var balance = await _web3.Eth.GetContractQueryHandler<ContractBalanceFunction>()
            .QueryAsync<BigInteger>(contract, new ContractBalanceFunction());

        return new BoardInfo(contract, owner, message, updates.ToString(),
            balance.ToString(), Web3.Convert.FromWei(balance));
    }

    public async Task<TxResult> SetMessageAsync(string message)
    {
        await EnsureCanWriteAsync();
        var contract = RequireContractAddress();

        // Nethereum estimates gas, signs with the configured key and waits until the tx is mined.
        var receipt = await _web3.Eth.GetContractTransactionHandler<SetMessageFunction>()
            .SendRequestAndWaitForReceiptAsync(contract, new SetMessageFunction { NewMessage = message });

        return ToResult(receipt);
    }

    public async Task<TxResult> DepositAsync(decimal amountEth)
    {
        if (amountEth <= 0 || amountEth > _options.MaxDepositEth)
            throw new InvalidOperationException($"Amount must be greater than 0 and at most {_options.MaxDepositEth} ETH.");

        await EnsureCanWriteAsync();
        var contract = RequireContractAddress();

        var receipt = await _web3.Eth.GetContractTransactionHandler<DepositFunction>()
            .SendRequestAndWaitForReceiptAsync(contract,
                new DepositFunction { AmountToSend = Web3.Convert.ToWei(amountEth) });

        return ToResult(receipt);
    }

    public async Task<IReadOnlyList<MessageUpdatedItem>> GetMessageUpdatedEventsAsync(ulong? fromBlock)
    {
        var contract = RequireContractAddress();

        var latest = (await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value;
        // Public RPC providers limit getLogs ranges, so default to a recent window.
        var start = fromBlock.HasValue
            ? new BigInteger(fromBlock.Value)
            : BigInteger.Max(BigInteger.Zero, latest - 5000);

        var evt = _web3.Eth.GetEvent<MessageUpdatedEventDto>(contract);
        var filter = evt.CreateFilterInput(new BlockParameter((ulong)start), BlockParameter.CreateLatest());
        var logs = await evt.GetAllChangesAsync(filter);

        return logs
            .Select(l => new MessageUpdatedItem(
                l.Event.Author,
                l.Event.NewMessage,
                l.Event.UpdateCount.ToString(),
                l.Log.BlockNumber.Value.ToString(),
                l.Log.TransactionHash))
            .ToList();
    }

    // ---------------------------------------------------------------- helpers

    private string RequireContractAddress()
    {
        if (string.IsNullOrWhiteSpace(_options.ContractAddress))
            throw new InvalidOperationException("Ethereum:ContractAddress is not configured. Deploy the contract first.");
        EnsureValidAddress(_options.ContractAddress);
        return _options.ContractAddress;
    }

    private static void EnsureValidAddress(string address)
    {
        if (!AddressUtil.Current.IsValidEthereumAddressHexFormat(address))
            throw new InvalidOperationException($"'{address}' is not a valid Ethereum address.");
    }

    /// <summary>Guards every write: key present, mainnet switch, and node really is the expected chain.</summary>
    private async Task EnsureCanWriteAsync()
    {
        if (_account is null)
            throw new InvalidOperationException("No signing key configured (Ethereum:PrivateKey). The API is read-only.");

        if (_options.ChainId == 1 && !_options.AllowMainnetWrites)
            throw new InvalidOperationException("Writes to Ethereum mainnet are disabled (Ethereum:AllowMainnetWrites=false).");

        if (_chainVerified) return;

        var nodeChainId = (long)(await _web3.Eth.ChainId.SendRequestAsync()).Value;
        if (nodeChainId != _options.ChainId)
            throw new InvalidOperationException(
                $"RPC node reports chain id {nodeChainId} but Ethereum:ChainId is {_options.ChainId}. Refusing to sign.");

        _chainVerified = true;
    }

    private static TxResult ToResult(TransactionReceipt receipt) => new(
        receipt.TransactionHash,
        receipt.BlockNumber.Value.ToString(),
        receipt.Status?.Value == BigInteger.One,
        receipt.GasUsed.Value.ToString());
}
