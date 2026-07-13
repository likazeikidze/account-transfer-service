using AccountTransferService.Domain.Enums;

namespace AccountTransferService.Domain.Entities;

public class Transfer
{
    public Guid Id { get; set; }
    public Guid SenderAccountId { get; set; }
    public Guid ReceiverAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public TransferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account? SenderAccount { get; set; }
    public Account? ReceiverAccount { get; set; }
}
