using AccountTransferService.Domain.Entities;

namespace AccountTransferService.Domain.Interfaces;

public interface ITransferService
{
    Task<Transfer> TransferAsync(Guid senderAccountId, Guid receiverAccountId, decimal amount, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transfer>> GetTransfersAsync(CancellationToken cancellationToken = default);
}
