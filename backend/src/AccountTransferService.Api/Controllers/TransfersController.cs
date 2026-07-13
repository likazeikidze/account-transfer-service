using AccountTransferService.Api.Dtos;
using AccountTransferService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccountTransferService.Api.Controllers;

[ApiController]
[Route("api/transfers")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransferHistoryItemDto>>> GetTransfers(CancellationToken cancellationToken)
    {
        var transfers = await _transferService.GetTransfersAsync(cancellationToken);

        var result = transfers.Select(t => new TransferHistoryItemDto
        {
            Id = t.Id,
            SenderAccountId = t.SenderAccountId,
            SenderAccountNumber = t.SenderAccount?.AccountNumber ?? string.Empty,
            ReceiverAccountId = t.ReceiverAccountId,
            ReceiverAccountNumber = t.ReceiverAccount?.AccountNumber ?? string.Empty,
            Amount = t.Amount,
            Currency = t.Currency,
            Status = t.Status.ToString(),
            CreatedAt = t.CreatedAt
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TransferResponseDto>> CreateTransfer(TransferRequestDto request, CancellationToken cancellationToken)
    {
        var transfer = await _transferService.TransferAsync(
            request.SenderAccountId,
            request.ReceiverAccountId,
            request.Amount,
            cancellationToken);

        var response = new TransferResponseDto
        {
            Id = transfer.Id,
            SenderAccountId = transfer.SenderAccountId,
            ReceiverAccountId = transfer.ReceiverAccountId,
            Amount = transfer.Amount,
            Currency = transfer.Currency,
            Status = transfer.Status.ToString(),
            CreatedAt = transfer.CreatedAt
        };

        return Ok(response);
    }
}
