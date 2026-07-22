# Finance Wallet API

A modular monolith .NET 8 Web API for wallet management with deposit, withdrawal, transfer, and ledger-based accounting.

## Architecture

- **Modular Monolith** with CQRS/MediatR
- **11 business modules** — Identity, Wallets, Transactions, Ledger, Audit, Admin, ExternalAccounts, Security, Risk, Notifications, Messaging
- **Shared Kernel** — cross-cutting abstractions, domain primitives, pipeline behaviors
- **Schema-per-module** database design
- **Ledger-first** double-entry accounting

## Solution Structure

```
src/
  FinanceWallet.Api              — API entry point
  FinanceWallet.Bootstrapper     — DI wiring
  FinanceWallet.Shared           — shared kernel
  FinanceWallet.Modules.*        — business modules
tests/
  FinanceWallet.UnitTests
  FinanceWallet.IntegrationTests
  FinanceWallet.ArchitectureTests
```

## Prerequisites

- .NET 8 SDK
- SQLite (no external database required for local dev)

## Getting Started

```bash
dotnet restore
dotnet build
dotnet run --project src/FinanceWallet.Api
```
