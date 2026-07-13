using System.ComponentModel.DataAnnotations;

namespace AccountTransferService.Api.Dtos;

public class TransferRequestDto : IValidatableObject
{
    [Required]
    public Guid SenderAccountId { get; set; }

    [Required]
    public Guid ReceiverAccountId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SenderAccountId == ReceiverAccountId)
        {
            yield return new ValidationResult(
                "Sender and receiver accounts must be different.",
                new[] { nameof(SenderAccountId), nameof(ReceiverAccountId) });
        }
    }
}
