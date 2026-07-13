namespace AccountTransferService.Domain.Exceptions;

public class InsufficientFundsException : DomainException
{
    public override string ErrorCode => "INSUFFICIENT_FUNDS";

    public InsufficientFundsException(Guid accountId)
        : base($"Account '{accountId}' does not have sufficient funds for this transfer.")
    {
    }
}
