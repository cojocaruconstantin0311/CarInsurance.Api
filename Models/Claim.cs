using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Models;

public class Claim
{
    public long Id { get; set; }

    [Required]
    public long CarId { get; set; }
    public Car Car { get; set; } = default!;

    [Required]
    public DateOnly ClaimDate { get; set; }

    [Required, MaxLength(1000)]
    public string Description { get; set; } = default!;

    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
}
