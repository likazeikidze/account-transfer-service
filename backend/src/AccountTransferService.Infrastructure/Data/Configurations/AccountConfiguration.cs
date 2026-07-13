using AccountTransferService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountTransferService.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountNumber)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.OwnerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Balance)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(3);
    }
}
