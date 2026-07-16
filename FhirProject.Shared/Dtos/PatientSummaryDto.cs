namespace FhirProject.Shared.Dtos;

public class PatientSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? BirthDate { get; set; }
}
