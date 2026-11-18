const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("MessageBoard", function () {
  let board, owner, alice;

  beforeEach(async () => {
    [owner, alice] = await ethers.getSigners();
    const F = await ethers.getContractFactory("MessageBoard");
    board = await F.deploy("first");
    await board.waitForDeployment();
  });

  it("stores the initial message and owner", async () => {
    expect(await board.getMessage()).to.equal("first");
    expect(await board.owner()).to.equal(owner.address);
  });

  it("updates the message and emits an event", async () => {
    await expect(board.connect(alice).setMessage("hi"))
      .to.emit(board, "MessageUpdated")
      .withArgs(alice.address, "hi", 1n);
    expect(await board.getMessage()).to.equal("hi");
    expect(await board.updateCount()).to.equal(1n);
  });

  it("rejects an empty message", async () => {
    await expect(board.setMessage("")).to.be.revertedWithCustomError(board, "EmptyMessage");
  });

  it("accepts deposits and lets only the owner withdraw", async () => {
    const amount = ethers.parseEther("1");
    await expect(board.connect(alice).deposit({ value: amount }))
      .to.emit(board, "Deposited")
      .withArgs(alice.address, amount);
    expect(await board.contractBalance()).to.equal(amount);

    await expect(board.connect(alice).withdrawAll()).to.be.revertedWithCustomError(board, "NotOwner");
    await expect(board.withdrawAll()).to.changeEtherBalances([board, owner], [-amount, amount]);
  });
});
