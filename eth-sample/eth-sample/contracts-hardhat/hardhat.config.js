require("@nomicfoundation/hardhat-toolbox");
require("dotenv").config();

const { SEPOLIA_RPC_URL, MAINNET_RPC_URL, DEPLOYER_PRIVATE_KEY } = process.env;
const accounts = DEPLOYER_PRIVATE_KEY ? [DEPLOYER_PRIVATE_KEY] : [];

/** @type import('hardhat/config').HardhatUserConfig */
module.exports = {
  solidity: {
    version: "0.8.24",
    settings: { optimizer: { enabled: true, runs: 200 } },
  },
  networks: {
    // `npx hardhat node` starts this on http://127.0.0.1:8545 (chainId 31337)
    localhost: { url: "http://127.0.0.1:8545" },

    // Public test network. Free test ETH from a Sepolia faucet.
    ...(SEPOLIA_RPC_URL ? { sepolia: { url: SEPOLIA_RPC_URL, accounts, chainId: 11155111 } } : {}),

    // Real Ethereum mainnet. Costs REAL ETH. Only enable after audit + testnet runs.
    ...(MAINNET_RPC_URL ? { mainnet: { url: MAINNET_RPC_URL, accounts, chainId: 1 } } : {}),
  },
};
