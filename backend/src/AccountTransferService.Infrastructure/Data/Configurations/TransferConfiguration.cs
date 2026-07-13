using AccountTransferService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountTransferService.Infrastructure.Data.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasOne(t => t.SenderAccount)
            .WithMany()
            .HasForeignKey(t => t.SenderAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ReceiverAccount)
            .WithMany()
            .HasForeignKey(t => t.ReceiverAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
