using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using CarInsurance.Api.Models;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            p.EndDate >= date
        );
    }

    public async Task<InsuranceClaimDto> RegisterInsuranceClaims(long carId, DateOnly claimDate, string description, double amount)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var policyId = await _db.Policies
            .Where(p => p.CarId == carId && p.StartDate <= claimDate && claimDate <= p.EndDate)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        if (policyId == 0) throw new KeyNotFoundException($"No insurance found for the specified date");


        InsuranceClaim insuranceClaim = new() { CarId = carId, Amount = amount, ClaimDate = claimDate, Description = description, PolicyId = policyId };


        _db.InsuranceClaims.Add(insuranceClaim);

        await _db.SaveChangesAsync();
        long a = insuranceClaim.Id;

        return new InsuranceClaimDto(insuranceClaim.Id, claimDate.ToString(), description, amount);
    }

    public async Task<List<HistoryDto>> ListClaimsAsync(long carId)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");


        return await _db.InsuranceClaims
         .Where(c => c.CarId == carId)
         .OrderBy(c => c.ClaimDate)
         .Select(c => new HistoryDto(
             c.Id,
             c.ClaimDate.ToString("yyyy-MM-dd"),
             c.Policy.StartDate.ToString("yyyy-MM-dd"),
             c.Policy.EndDate.ToString("yyyy-MM-dd"),
             c.Description,
             c.Amount))
         .ToListAsync();
    }
}
