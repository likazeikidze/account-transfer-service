namespace AccountTransferService.Api.Dtos;

public class TransferResponseDto
{
    public Guid Id { get; set; }
    public Guid SenderAccountId { get; set; }
    public Guid ReceiverAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
