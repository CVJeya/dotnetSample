const fs = require("fs");
const path = require("path");
const hre = require("hardhat");

async function main() {
  const [deployer] = await hre.ethers.getSigners();
  const network = await hre.ethers.provider.getNetwork();
  console.log(`Network   : ${hre.network.name} (chainId ${network.chainId})`);
  console.log(`Deployer  : ${deployer.address}`);

  const factory = await hre.ethers.getContractFactory("MessageBoard");
  const contract = await factory.deploy("Hello from Hardhat");
  await contract.waitForDeployment();

  const address = await contract.getAddress();
  console.log(`MessageBoard deployed to: ${address}`);

  // Export ABI + address so the DApp and the .NET API can both use them.
  const artifact = await hre.artifacts.readArtifact("MessageBoard");
  const outDir = path.join(__dirname, "..", "deployments");
  fs.mkdirSync(outDir, { recursive: true });

  fs.writeFileSync(
    path.join(outDir, `${hre.network.name}.json`),
    JSON.stringify(
      { network: hre.network.name, chainId: Number(network.chainId), address, abi: artifact.abi },
      null,
      2
    )
  );

  // Copy to the DApp so it can load the address/ABI without manual edits.
  const dappFile = path.join(__dirname, "..", "..", "dapp", "contract.json");
  fs.writeFileSync(
    dappFile,
    JSON.stringify({ chainId: Number(network.chainId), address, abi: artifact.abi }, null, 2)
  );
  console.log(`Saved deployments/${hre.network.name}.json and dapp/contract.json`);
}

main().catch((e) => {
  console.error(e);
  process.exitCode = 1;
});
