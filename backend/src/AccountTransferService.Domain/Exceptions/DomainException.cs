namespace AccountTransferService.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public abstract string ErrorCode { get; }

    protected DomainException(string message) : base(message)
    {
    }
}
