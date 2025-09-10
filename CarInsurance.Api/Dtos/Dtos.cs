namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);

public record InsuranceClaimDto(long Id, string ClaimDate, string Description, double Amount);

public record HistoryDto(long Id, string ClaimDate, string StartDate, string EndDate, string Description, double Amount);