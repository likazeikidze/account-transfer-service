namespace AccountTransferService.Domain.Exceptions;

public class AccountNotFoundException : DomainException
{
    public override string ErrorCode => "ACCOUNT_NOT_FOUND";

    public AccountNotFoundException(Guid accountId)
        : base($"Account '{accountId}' does not exist.")
    {
    }
}
