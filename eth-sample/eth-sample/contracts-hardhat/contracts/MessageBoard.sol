// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

/// @title MessageBoard
/// @notice Small sample contract used to demo Nethereum (ASP.NET Core) and a browser DApp.
///         It shows: a read (view) call, a write transaction, a payable function,
///         events, access control and custom errors.
contract MessageBoard {
    // ---------------------------------------------------------------------
    // State
    // ---------------------------------------------------------------------
    address public owner;
    string private _message;
    uint256 public updateCount;
    mapping(address => uint256) public deposits;

    // ---------------------------------------------------------------------
    // Events (Nethereum can decode these into typed DTOs)
    // ---------------------------------------------------------------------
    event MessageUpdated(address indexed author, string newMessage, uint256 updateCount);
    event Deposited(address indexed from, uint256 amount);
    event Withdrawn(address indexed to, uint256 amount);

    // ---------------------------------------------------------------------
    // Errors
    // ---------------------------------------------------------------------
    error NotOwner();
    error EmptyMessage();
    error NothingToWithdraw();
    error TransferFailed();

    modifier onlyOwner() {
        if (msg.sender != owner) revert NotOwner();
        _;
    }

    constructor(string memory initialMessage) {
        owner = msg.sender;
        _message = initialMessage;
    }

    // ---------------------------------------------------------------------
    // Reads (free, no gas when called off-chain)
    // ---------------------------------------------------------------------
    function getMessage() external view returns (string memory) {
        return _message;
    }

    function contractBalance() external view returns (uint256) {
        return address(this).balance;
    }

    // ---------------------------------------------------------------------
    // Writes (need a signed transaction + gas)
    // ---------------------------------------------------------------------
    function setMessage(string calldata newMessage) external {
        if (bytes(newMessage).length == 0) revert EmptyMessage();
        _message = newMessage;
        unchecked {
            updateCount += 1;
        }
        emit MessageUpdated(msg.sender, newMessage, updateCount);
    }

    /// @notice Send ETH to the contract; tracked per sender.
    function deposit() external payable {
        deposits[msg.sender] += msg.value;
        emit Deposited(msg.sender, msg.value);
    }

    /// @notice Owner pulls all ETH held by the contract.
    function withdrawAll() external onlyOwner {
        uint256 amount = address(this).balance;
        if (amount == 0) revert NothingToWithdraw();
        (bool ok, ) = payable(owner).call{value: amount}("");
        if (!ok) revert TransferFailed();
        emit Withdrawn(owner, amount);
    }
}
