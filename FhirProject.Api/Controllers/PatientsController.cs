using FhirProject.Api.Services;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IFhirService _fhirService;

    public PatientsController(IFhirService fhirService)
    {
        _fhirService = fhirService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Query parameter 'name' is required.");
        }

        try
        {
            var results = await _fhirService.SearchPatientsAsync(name);
            return Ok(results);
        }
        catch (Exception ex) when (ex is FhirOperationException or HttpRequestException or TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                "The public FHIR server did not respond. Please try again in a moment.");
        }
    }

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> Timeline(string id)
    {
        try
        {
            var timeline = await _fhirService.GetPatientTimelineAsync(id);
            if (timeline is null)
            {
                return NotFound();
            }

            return Ok(timeline);
        }
        catch (Exception ex) when (ex is FhirOperationException or HttpRequestException or TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status502BadGateway,
                "The public FHIR server did not respond. Please try again in a moment.");
        }
    }
}
