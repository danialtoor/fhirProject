using FhirProject.Shared.Dtos;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace FhirProject.Api.Services;

public interface IFhirService
{
    Task<List<PatientSummaryDto>> SearchPatientsAsync(string name);
    Task<PatientTimelineDto?> GetPatientTimelineAsync(string patientId);
}

public class FhirService : IFhirService
{
    private readonly FhirClient _client;

    public FhirService(HttpClient httpClient)
    {
        _client = new FhirClient(
            "https://hapi.fhir.org/baseR4",
            httpClient,
            new FhirClientSettings { PreferredFormat = ResourceFormat.Json, Timeout = 30_000 });
    }

    public async Task<List<PatientSummaryDto>> SearchPatientsAsync(string name)
    {
        var searchParams = new SearchParams()
            .Add("name", name)
            .Add("_count", "20");

        var bundle = await _client.SearchAsync<Patient>(searchParams);

        var results = new List<PatientSummaryDto>();
        foreach (var entry in bundle?.Entry ?? [])
        {
            if (entry.Resource is Patient patient)
            {
                results.Add(new PatientSummaryDto
                {
                    Id = patient.Id ?? string.Empty,
                    FullName = FormatName(patient),
                    Gender = patient.Gender?.ToString(),
                    BirthDate = patient.BirthDate
                });
            }
        }

        return results;
    }

    public async Task<PatientTimelineDto?> GetPatientTimelineAsync(string patientId)
    {
        var searchParams = new SearchParams()
            .Add("_id", patientId)
            .Add("_revinclude", "Encounter:subject")
            .Add("_revinclude", "Condition:subject")
            .Add("_revinclude", "Observation:subject")
            .Add("_revinclude", "MedicationRequest:subject");

        var bundle = await _client.SearchAsync<Patient>(searchParams);
        var resources = bundle?.Entry.Select(e => e.Resource).ToList() ?? [];

        var patient = resources.OfType<Patient>().FirstOrDefault();
        if (patient is null)
        {
            return null;
        }

        var timeline = new PatientTimelineDto
        {
            PatientId = patient.Id ?? patientId,
            PatientName = FormatName(patient)
        };

        foreach (var encounter in resources.OfType<Encounter>())
        {
            timeline.Encounters.Add(new EncounterDto
            {
                Status = encounter.Status?.ToString(),
                Type = encounter.Type.FirstOrDefault()?.Text
                       ?? encounter.Type.FirstOrDefault()?.Coding.FirstOrDefault()?.Display,
                PeriodStart = encounter.Period?.Start,
                PeriodEnd = encounter.Period?.End
            });
        }

        foreach (var condition in resources.OfType<Condition>())
        {
            var coding = condition.Code?.Coding.FirstOrDefault(c => c.System == "http://snomed.info/sct");
            timeline.Conditions.Add(new ConditionDto
            {
                Text = condition.Code?.Text ?? coding?.Display ?? "(unknown condition)",
                SnomedCode = coding?.Code
            });
        }

        foreach (var observation in resources.OfType<Observation>())
        {
            var coding = observation.Code?.Coding.FirstOrDefault(c => c.System == "http://loinc.org");
            var (value, unit) = observation.Value switch
            {
                Quantity q => (q.Value?.ToString(), q.Unit),
                FhirString s => (s.Value, null),
                CodeableConcept cc => (cc.Text ?? cc.Coding.FirstOrDefault()?.Display, null),
                _ => (null, null)
            };

            timeline.Observations.Add(new ObservationDto
            {
                Text = observation.Code?.Text ?? coding?.Display ?? "(unknown observation)",
                LoincCode = coding?.Code,
                Value = value,
                Unit = unit,
                EffectiveDate = (observation.Effective as FhirDateTime)?.Value
            });
        }

        foreach (var medicationRequest in resources.OfType<MedicationRequest>())
        {
            var cc = medicationRequest.Medication as CodeableConcept;
            var coding = cc?.Coding.FirstOrDefault(c => c.System == "http://www.nlm.nih.gov/research/umls/rxnorm");
            timeline.Medications.Add(new MedicationRequestDto
            {
                Text = cc?.Text ?? coding?.Display ?? "(unknown medication)",
                RxNormCode = coding?.Code,
                Status = medicationRequest.Status?.ToString()
            });
        }

        return timeline;
    }

    private static string FormatName(Patient patient)
    {
        var name = patient.Name.FirstOrDefault();
        if (name is null)
        {
            return "(unknown)";
        }

        var given = string.Join(" ", name.Given ?? []);
        return string.IsNullOrWhiteSpace(given) ? name.Family ?? "(unknown)" : $"{given} {name.Family}".Trim();
    }
}
