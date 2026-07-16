namespace FhirProject.Shared.Dtos;

public class PatientTimelineDto
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public List<EncounterDto> Encounters { get; set; } = [];
    public List<ConditionDto> Conditions { get; set; } = [];
    public List<ObservationDto> Observations { get; set; } = [];
    public List<MedicationRequestDto> Medications { get; set; } = [];
}

public class EncounterDto
{
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? PeriodStart { get; set; }
    public string? PeriodEnd { get; set; }
}

public class ConditionDto
{
    public string Text { get; set; } = string.Empty;
    public string? SnomedCode { get; set; }
}

public class ObservationDto
{
    public string Text { get; set; } = string.Empty;
    public string? LoincCode { get; set; }
    public string? Value { get; set; }
    public string? Unit { get; set; }
    public string? EffectiveDate { get; set; }
}

public class MedicationRequestDto
{
    public string Text { get; set; } = string.Empty;
    public string? RxNormCode { get; set; }
    public string? Status { get; set; }
}
