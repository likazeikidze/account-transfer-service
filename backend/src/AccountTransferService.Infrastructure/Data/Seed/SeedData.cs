using AccountTransferService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountTransferService.Infrastructure.Data.Seed;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Accounts.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        context.Accounts.AddRange(
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-1001",
                OwnerName = "Lika Zeikidze",
                Balance = 1000.00m,
                Currency = "USD",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-1002",
                OwnerName = "Bob Smith",
                Balance = 500.00m,
                Currency = "USD",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-1003",
                OwnerName = "Charlie Davis",
                Balance = 250.00m,
                Currency = "USD",
                CreatedAt = now,
                UpdatedAt = now
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-1004",
                OwnerName = "Diana Lee",
                Balance = 0.00m,
                Currency = "USD",
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        await context.SaveChangesAsync(cancellationToken);
    }
}
