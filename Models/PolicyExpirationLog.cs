using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Models;

public class PolicyExpirationLog
{
    public long Id { get; set; }

    [Required]
    public long PolicyId { get; set; }
    public InsurancePolicy Policy { get; set; } = default!;

    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
}
