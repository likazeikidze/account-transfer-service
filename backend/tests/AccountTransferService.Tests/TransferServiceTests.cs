using AccountTransferService.Domain.Entities;
using AccountTransferService.Domain.Exceptions;
using AccountTransferService.Infrastructure.Data;
using AccountTransferService.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountTransferService.Tests;

public class TransferServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly TransferService _sut;

    public TransferServiceTests()
    {
        // A real relational provider (SQLite, in-memory) is used instead of EF Core's
        // InMemory provider because the transfer logic relies on ExecuteUpdateAsync and
        // explicit transactions, neither of which the InMemory provider supports.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _sut = new TransferService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<Account> CreateAccountAsync(decimal balance)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = $"ACC-{Guid.NewGuid():N}"[..12],
            OwnerName = "Test Owner",
            Balance = balance,
            Currency = "USD",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    // ExecuteUpdateAsync writes straight to the database, bypassing the change
    // tracker, so already-tracked entities must be re-fetched untracked to observe it.
    private Task<decimal> GetBalanceAsync(Guid accountId) =>
        _context.Accounts.AsNoTracking().Where(a => a.Id == accountId).Select(a => a.Balance).SingleAsync();

    [Fact]
    public async Task TransferAsync_WithSufficientFunds_DebitsSenderAndCreditsReceiver()
    {
        var sender = await CreateAccountAsync(500m);
        var receiver = await CreateAccountAsync(100m);

        var transfer = await _sut.TransferAsync(sender.Id, receiver.Id, 200m);

        Assert.Equal(200m, transfer.Amount);

        Assert.Equal(300m, await GetBalanceAsync(sender.Id));
        Assert.Equal(300m, await GetBalanceAsync(receiver.Id));
    }

    [Fact]
    public async Task TransferAsync_WithInsufficientFunds_ThrowsAndLeavesBalancesUnchanged()
    {
        var sender = await CreateAccountAsync(50m);
        var receiver = await CreateAccountAsync(100m);

        await Assert.ThrowsAsync<InsufficientFundsException>(
            () => _sut.TransferAsync(sender.Id, receiver.Id, 200m));

        Assert.Equal(50m, await GetBalanceAsync(sender.Id));
        Assert.Equal(100m, await GetBalanceAsync(receiver.Id));
    }

    [Fact]
    public async Task TransferAsync_WithUnknownSender_ThrowsAccountNotFound()
    {
        var receiver = await CreateAccountAsync(100m);

        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _sut.TransferAsync(Guid.NewGuid(), receiver.Id, 10m));
    }

    [Fact]
    public async Task TransferAsync_WithUnknownReceiver_ThrowsAccountNotFound()
    {
        var sender = await CreateAccountAsync(100m);

        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => _sut.TransferAsync(sender.Id, Guid.NewGuid(), 10m));
    }

    [Fact]
    public async Task TransferAsync_OnSuccess_RecordsTransferHistoryEntry()
    {
        var sender = await CreateAccountAsync(500m);
        var receiver = await CreateAccountAsync(100m);

        await _sut.TransferAsync(sender.Id, receiver.Id, 150m);

        var history = await _sut.GetTransfersAsync();

        Assert.Single(history);
        Assert.Equal(150m, history[0].Amount);
        Assert.Equal(sender.Id, history[0].SenderAccountId);
        Assert.Equal(receiver.Id, history[0].ReceiverAccountId);
    }
}
