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
        try
        {
            var parsed = CarService.ParseStrictDate(date); 
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message); 
        }
        catch (KeyNotFoundException)
        {
            return NotFound(); 
        }
    }

    [HttpPost]
    [Route("cars/{carId:long}/claims")]
    public async Task<ActionResult<ClaimResponse>> RegisterClaim(
    long carId,
    [FromQuery] string claimDate,
    [FromQuery] string description,
    [FromQuery] decimal amount)
    {
        if (!DateOnly.TryParse(claimDate, out _))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        if (string.IsNullOrWhiteSpace(description))
            return BadRequest("Description is required.");
        if (amount < 0)
            return BadRequest("Amount must be non-negative.");

        try
        {
            var resp = await _service.RegisterClaimAsync(carId, claimDate, description, amount);
            return Created("", resp);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("cars/{carId:long}/history")]
    public async Task<IActionResult> GetHistory([FromRoute] long carId)
    {
        try
        {
            var resp = await _service.GetHistoryAsync(carId);
            return Ok(resp);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}


