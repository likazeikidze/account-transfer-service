namespace AccountTransferService.Domain.Entities;

public class Account
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
