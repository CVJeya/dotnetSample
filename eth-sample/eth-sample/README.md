# Ethereum sample: Solidity + ASP.NET Core (Nethereum) + browser DApp

```
Browser DApp (MetaMask, ethers.js) ──┐
                                     ├──► Ethereum node (JSON-RPC) ──► MessageBoard contract
ASP.NET Core API (Nethereum) ────────┘     local Hardhat | Sepolia | mainnet
```

| Folder | What it is |
|---|---|
| `contracts-hardhat/` | `MessageBoard.sol`, Hardhat config, deploy script, unit tests |
| `NethereumDemo.Api/` | ASP.NET Core minimal API that reads/writes the contract with Nethereum |
| `dapp/` | Single-page DApp (`index.html`) to test the same contract from the browser |

The contract has a view call (`getMessage`), a write (`setMessage`), a payable function (`deposit`),
an owner-only function (`withdrawAll`), events and custom errors, so each Nethereum pattern is covered.

## Prerequisites
- Node.js 18+ (Hardhat), .NET 8 SDK (or change `TargetFramework` to `net10.0`), MetaMask browser extension.

## A. Run everything locally (no real money)

**1. Start a local chain** (terminal 1). It prints 20 funded test accounts and keeps running:
```bash
cd contracts-hardhat
npm install
npx hardhat test          # optional: run the contract unit tests
npx hardhat node          # RPC at http://127.0.0.1:8545, chainId 31337
```

**2. Deploy the contract** (terminal 2):
```bash
cd contracts-hardhat
npm run deploy:local
```
The script prints the contract address and writes `dapp/contract.json` (address + ABI).
On a fresh node the first deployment is always `0x5FbDB2315678afecb367f032d93F642f64180aa3`,
which is already set in `appsettings.Development.json`. If your address differs, update it there.

**3. Run the ASP.NET Core API** (terminal 3):
```bash
cd NethereumDemo.Api
dotnet run
```
Open `/swagger` on the URL shown in the console. Try:
- `GET /api/eth/network` (chain id, block number, signer)
- `GET /api/board` (message, owner, update count, contract balance)
- `POST /api/board/message` with `{ "message": "Hello from .NET" }`
- `POST /api/board/deposit` with `{ "amountEth": 0.5 }`
- `GET /api/board/events` (decoded `MessageUpdated` logs)

`appsettings.Development.json` uses Hardhat's well-known account #0 key. It is public and only valid on local test chains.

**4. Test with the DApp**
```bash
cd dapp
npx serve .        # or: python3 -m http.server 3000
```
1. Open the served URL (not `file://`, because `contract.json` is fetched).
2. In MetaMask add the network: RPC `http://127.0.0.1:8545`, chain id `31337`, symbol `ETH`
   (the **Switch to local Hardhat** button does this for you).
3. Import a test account: MetaMask → Import account → paste a private key printed by `hardhat node`.
   Account #0 is the contract owner, so it can use **Withdraw all**.
4. Connect, load the contract, then set a message or deposit ETH. Confirm in MetaMask.
5. Call `GET /api/board` in Swagger: the API sees what the DApp wrote, and vice versa. Updates also appear in the DApp's event list.

## B. Public testnet (Sepolia)
1. Get an RPC URL (Infura, Alchemy, etc.) and test ETH from a Sepolia faucet using a **dedicated test wallet**.
2. `contracts-hardhat/.env`: set `SEPOLIA_RPC_URL` and `DEPLOYER_PRIVATE_KEY` (see `.env.example`).
3. `npm run deploy:sepolia`, then note the printed address.
4. API configuration (keep the key out of source control):
   ```bash
   cd NethereumDemo.Api
   dotnet user-secrets set "Ethereum:RpcUrl" "https://sepolia.infura.io/v3/<key>"
   dotnet user-secrets set "Ethereum:ChainId" "11155111"
   dotnet user-secrets set "Ethereum:ContractAddress" "<deployed address>"
   dotnet user-secrets set "Ethereum:PrivateKey" "<test wallet key>"
   ```
   Note: `appsettings.Development.json` overrides `appsettings.json`, and user-secrets override both, so secrets win in Development.
5. DApp: switch MetaMask to Sepolia, paste the contract address, and load it.

## C. Ethereum mainnet
Same flow with `MAINNET_RPC_URL` and chain id `1`, but this spends real ETH and deployed contracts are immutable.
Before doing this: get the contract audited, rehearse on Sepolia, and use a hardware wallet or a managed key (Azure Key Vault)
rather than a raw private key. The API refuses mainnet writes unless `Ethereum:AllowMainnetWrites` is `true`;
a read-only API (no `PrivateKey`) against mainnet is the safe way to start.

## Troubleshooting
- **No contract at address / empty reads**: MetaMask or the API is on a different network than the deployment. Restarting `hardhat node` wipes the chain, so redeploy.
- **"nonce too low" in the API after restarting the node**: restart the API too (Nethereum caches the nonce).
- **"RPC node reports chain id X but Ethereum:ChainId is Y"**: the API's safety check; fix the configuration.
- **MetaMask shows a wrong nonce after resetting the local node**: MetaMask → Settings → Advanced → Clear activity tab data.
- **Version note**: the code targets Nethereum 4.29.0, ethers 6.13.4 and Hardhat 2.x. Newer major versions may need small changes.


Option 1: Hardhat node (what the sample uses)

Requires Node.js 18+.

bash
cd contracts-hardhat
npm install
npx hardhat node

This starts a JSON-RPC server at http://127.0.0.1:8545 with chain ID 31337. It prints 20 test accounts, each with 10,000 test ETH, along with their private keys. Keep this terminal open.

In a second terminal, deploy the contract:

bash
cd contracts-hardhat
npm run deploy:local

The printed address is your running contract. To confirm the server is alive:

bash
curl -X POST http://127.0.0.1:8545 -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"eth_blockNumber","params":[],"id":1}'

On Windows PowerShell, use curl.exe or Invoke-RestMethod instead.

Option 2: Anvil (Foundry)

Anvil is a faster alternative and works the same way from your app’s point of view:

bash
curl -L https://foundry.paradigm.xyz | bash   # then: foundryup
anvil                                          # http://127.0.0.1:8545, chainId 31337
forge create src/MessageBoard.sol:MessageBoard --rpc-url http://127.0.0.1:8545 \
  --private-key <anvil account key> --broadcast --constructor-args "Hello"

This needs a Foundry project, since forge compiles from src/. On Windows, use WSL for the installer.

Option 3: Quick experiments without installing anything

Open Remix IDE in your browser, paste the contract, compile it, and deploy to the Remix VM, an in-browser chain. You can call functions from its UI. Nothing is reachable from outside the browser, so your .NET API can’t use it.

Option 4: A real Geth node in dev mode

geth --dev --http starts a single-node private chain with instant mining. It uses real client software, but it’s more setup than Hardhat or Anvil and has no test helpers, so it’s rarely needed for development.

Connecting your app and interacting
Nethereum API: set Ethereum:RpcUrl to http://127.0.0.1:8545, ChainId to 31337, and ContractAddress to the address you deployed.
MetaMask: add a network with that RPC URL and chain ID, then import one of the printed private keys.
Directly from the terminal: npx hardhat console --network localhost. For example:
js
  const c = await ethers.getContractAt("MessageBoard", "<address>");
  await c.getMessage();
  await (await c.setMessage("hi")).wait();
Things to know
The local chain lives in memory. Stopping or restarting the node wipes everything, so redeploy afterwards. A fresh node gives the same first address, 0x5FbDB2315678afecb367f032d93F642f64180aa3.
The printed private keys are public and well known. Never send real funds to those addresses on any real network.
If something is unreachable, check that the node terminal is still running, that port 8545 isn’t taken by something else, and that your app and wallet are on chain ID 31337.
To let another machine reach your node, Hardhat needs npx hardhat node --hostname 0.0.0.0. Only do this on a trusted network, because the accounts are unlocked and anyone who can reach the port can spend from them.