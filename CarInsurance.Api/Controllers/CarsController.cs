using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("cars/{carId:long}/claims")]
    public async Task<ActionResult<InsuranceClaimDto>> RegisterInsuranceClaims(long carId, string claimDate, string description, double amount)
    {
        if (!DateOnly.TryParse(claimDate, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            InsuranceClaimDto insuranceClaims = await _service.RegisterInsuranceClaims(carId, parsed, description, amount);
            return Ok(insuranceClaims);
        }
        catch (KeyNotFoundException e)
        {
            return BadRequest(e.Message);
        }

    }

    [HttpGet("cars/{carId:long}/history")]
    public async Task<ActionResult<List<HistoryDto>>> GetClaimsHistory(long carId)
    {
        var result = await _service.ListClaimsAsync(carId);
        if (result.Count == 0) return Ok($"No insurance claims for car {carId}");
        return Ok(result);
    }
}
