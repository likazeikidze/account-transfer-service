namespace AccountTransferService.Api.Dtos;

public class TransferHistoryItemDto
{
    public Guid Id { get; set; }
    public Guid SenderAccountId { get; set; }
    public string SenderAccountNumber { get; set; } = string.Empty;
    public Guid ReceiverAccountId { get; set; }
    public string ReceiverAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
