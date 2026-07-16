namespace FhirProject.Shared.Dtos;

public class Hl7ConversionResultDto
{
    public List<Hl7SegmentDto> Segments { get; set; } = [];
    public List<string> FhirResourcesJson { get; set; } = [];
}

public class Hl7SegmentDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Fields { get; set; } = [];
}

public class Hl7RawMessageRequest
{
    public string RawMessage { get; set; } = string.Empty;
}
