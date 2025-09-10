namespace CarInsurance.Api.Models;

public class InsuranceClaim
{
    public long Id { get; set; }

    public long CarId { get; set; }

 
    public DateOnly ClaimDate { get; set; }

    public string? Description { get; set; }

    public double Amount { get; set; }

    public long PolicyId { get; set; }

    public InsurancePolicy Policy { get; set; } = default!;

}