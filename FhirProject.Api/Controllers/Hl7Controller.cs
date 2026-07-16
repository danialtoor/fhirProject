using FhirProject.Api.Services;
using FhirProject.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/hl7")]
public class Hl7Controller : ControllerBase
{
    private readonly IHl7V2ParserService _parserService;

    public Hl7Controller(IHl7V2ParserService parserService)
    {
        _parserService = parserService;
    }

    [HttpPost("adt")]
    public IActionResult ParseAdt([FromBody] Hl7RawMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawMessage))
        {
            return BadRequest("A raw HL7 v2 ADT message is required.");
        }

        try
        {
            return Ok(_parserService.ParseAdt(request.RawMessage));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("oru")]
    public IActionResult ParseOru([FromBody] Hl7RawMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RawMessage))
        {
            return BadRequest("A raw HL7 v2 ORU message is required.");
        }

        try
        {
            return Ok(_parserService.ParseOru(request.RawMessage));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
