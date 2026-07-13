namespace AccountTransferService.Api.Dtos;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
}
