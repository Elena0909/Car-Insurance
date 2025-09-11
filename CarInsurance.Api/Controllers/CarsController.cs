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
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        if (carId < 1)
            return BadRequest("Invalid car id. Make sure it is greater than 0.");

        //1885 - year of the first car invented
        if (parsedDate.Month <= 0 || parsedDate.Month > 12 || parsedDate.Day <= 0 || parsedDate.Day > 31 || parsedDate.Year < 1885)
            return BadRequest($"The specified date is invalid. Make sure it has the format YYYY-MM-DD and is a valid date.");


        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsedDate);
            return Ok(new InsuranceValidityResponse(carId, parsedDate.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
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
        try
        {
            var result = await _service.ListClaimsAsync(carId);
            if (result.Count == 0) return NotFound($"No insurance claims for car {carId}");
            return Ok(result);
        }
        catch(KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
