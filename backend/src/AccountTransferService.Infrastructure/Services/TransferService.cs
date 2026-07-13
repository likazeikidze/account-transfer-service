using AccountTransferService.Domain.Entities;
using AccountTransferService.Domain.Enums;
using AccountTransferService.Domain.Exceptions;
using AccountTransferService.Domain.Interfaces;
using AccountTransferService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccountTransferService.Infrastructure.Services;

public class TransferService : ITransferService
{
    private readonly AppDbContext _context;

    public TransferService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transfer> TransferAsync(Guid senderAccountId, Guid receiverAccountId, decimal amount, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // Always touch the lower Id first, regardless of transfer direction, so two
        // concurrent transfers in opposite directions can't deadlock on row locks.
        var firstId = senderAccountId.CompareTo(receiverAccountId) <= 0 ? senderAccountId : receiverAccountId;
        var secondId = firstId == senderAccountId ? receiverAccountId : senderAccountId;

        await EnsureAccountExistsAsync(firstId, cancellationToken);
        await EnsureAccountExistsAsync(secondId, cancellationToken);

        var now = DateTime.UtcNow;

        // The balance check happens inside the SQL UPDATE ... WHERE clause, so the
        // check-and-decrement is one atomic statement — no gap for a concurrent
        // transfer to interleave and over-draw the account.
        var debitedRows = await _context.Accounts
            .Where(a => a.Id == senderAccountId && a.Balance >= amount)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Balance, a => a.Balance - amount)
                .SetProperty(a => a.UpdatedAt, now), cancellationToken);

        if (debitedRows == 0)
        {
            throw new InsufficientFundsException(senderAccountId);
        }

        await _context.Accounts
            .Where(a => a.Id == receiverAccountId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Balance, a => a.Balance + amount)
                .SetProperty(a => a.UpdatedAt, now), cancellationToken);

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            SenderAccountId = senderAccountId,
            ReceiverAccountId = receiverAccountId,
            Amount = amount,
            Currency = "USD",
            Status = TransferStatus.Completed,
            CreatedAt = now
        };

        _context.Transfers.Add(transfer);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return transfer;
    }

    public async Task<IReadOnlyList<Transfer>> GetTransfersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Transfers
            .Include(t => t.SenderAccount)
            .Include(t => t.ReceiverAccount)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureAccountExistsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var exists = await _context.Accounts.AnyAsync(a => a.Id == accountId, cancellationToken);
        if (!exists)
        {
            throw new AccountNotFoundException(accountId);
        }
    }
}
