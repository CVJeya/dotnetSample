namespace NethereumDemo.Api.Models;

public class EthereumOptions
{
    public const string SectionName = "Ethereum";

    /// <summary>JSON-RPC endpoint: local Hardhat node, Sepolia (Infura/Alchemy) or mainnet.</summary>
    public string RpcUrl { get; set; } = "http://127.0.0.1:8545";

    /// <summary>Expected chain id. 31337 = Hardhat, 11155111 = Sepolia, 1 = Ethereum mainnet.</summary>
    public long ChainId { get; set; } = 31337;

    /// <summary>Address of the deployed MessageBoard contract.</summary>
    public string ContractAddress { get; set; } = string.Empty;

    /// <summary>
    /// Optional signing key. Leave empty for read-only mode.
    /// Store it with `dotnet user-secrets` or Azure Key Vault, never in source control.
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>Safety switch: writes on chain id 1 are refused unless this is true.</summary>
    public bool AllowMainnetWrites { get; set; } = false;

    /// <summary>Upper limit for the /deposit endpoint, in ETH.</summary>
    public decimal MaxDepositEth { get; set; } = 1m;
}
