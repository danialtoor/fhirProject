using System.Text.Json;
using FhirProject.Shared.Dtos;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirProject.Api.Services;

public interface IHl7V2ParserService
{
    Hl7ConversionResultDto ParseAdt(string rawMessage);
    Hl7ConversionResultDto ParseOru(string rawMessage);
}

public class Hl7V2ParserService : IHl7V2ParserService
{
    private static readonly JsonSerializerOptions FhirJsonOptions =
        new JsonSerializerOptions { WriteIndented = true }.ForFhir(ModelInfo.ModelInspector);

    public Hl7ConversionResultDto ParseAdt(string rawMessage)
    {
        var message = new Hl7Message(rawMessage);
        var pid = message.GetSegment("PID")
                  ?? throw new InvalidOperationException("Message is missing a required PID segment.");
        var pv1 = message.GetSegment("PV1");

        var patient = BuildPatientFromPid(pid);
        var resources = new List<Resource> { patient };

        if (pv1 is not null)
        {
            resources.Add(BuildEncounterFromPv1(pv1, patient));
        }

        return BuildResult(message, resources);
    }

    public Hl7ConversionResultDto ParseOru(string rawMessage)
    {
        var message = new Hl7Message(rawMessage);
        var pid = message.GetSegment("PID")
                  ?? throw new InvalidOperationException("Message is missing a required PID segment.");

        var patient = BuildPatientFromPid(pid);
        var resources = new List<Resource> { patient };
        resources.AddRange(message.GetSegments("OBX").Select(obx => BuildObservationFromObx(obx, patient)));

        return BuildResult(message, resources);
    }

    private static Patient BuildPatientFromPid(Hl7Segment pid)
    {
        // PID-3: Patient Identifier List, component 1 = the identifier value (MRN)
        var mrn = pid.GetComponent(3, 1);
        // PID-5: Patient Name, Family^Given
        var family = pid.GetComponent(5, 1);
        var given = pid.GetComponent(5, 2);
        // PID-7: Date of Birth, YYYYMMDD
        var dob = pid.GetField(7);
        // PID-8: Administrative Sex
        var sex = pid.GetField(8);

        var patientId = string.IsNullOrWhiteSpace(mrn) ? Guid.NewGuid().ToString() : mrn;

        var patient = new Patient
        {
            Id = patientId,
            Name = [new HumanName { Family = family, Given = string.IsNullOrWhiteSpace(given) ? [] : [given] }],
            BirthDate = FormatHl7Date(dob),
            Gender = sex switch
            {
                "M" => AdministrativeGender.Male,
                "F" => AdministrativeGender.Female,
                "O" => AdministrativeGender.Other,
                _ => AdministrativeGender.Unknown
            }
        };

        if (!string.IsNullOrWhiteSpace(mrn))
        {
            patient.Identifier.Add(new Identifier { System = "urn:hl7v2:PID-3", Value = mrn });
        }

        return patient;
    }

    private static Encounter BuildEncounterFromPv1(Hl7Segment pv1, Patient patient)
    {
        // PV1-2: Patient Class (I=Inpatient, O=Outpatient, E=Emergency)
        var patientClass = pv1.GetField(2);
        // PV1-19: Visit Number
        var visitNumber = pv1.GetComponent(19, 1);

        var encounter = new Encounter
        {
            Status = Encounter.EncounterStatus.Unknown,
            Class = new Coding
            {
                System = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                Code = MapPatientClass(patientClass),
                Display = patientClass
            },
            Subject = new ResourceReference($"Patient/{patient.Id}")
        };

        if (!string.IsNullOrWhiteSpace(visitNumber))
        {
            encounter.Identifier.Add(new Identifier { System = "urn:hl7v2:PV1-19", Value = visitNumber });
        }

        return encounter;
    }

    private static string MapPatientClass(string code) => code switch
    {
        "I" => "IMP",
        "O" => "AMB",
        "E" => "EMER",
        _ => "AMB"
    };

    private static Observation BuildObservationFromObx(Hl7Segment obx, Patient patient)
    {
        // OBX-3: Observation Identifier, Code^Text^CodingSystem (LN = LOINC)
        var code = obx.GetComponent(3, 1);
        var text = obx.GetComponent(3, 2);
        var codingSystem = obx.GetComponent(3, 3);
        // OBX-5: Observation Value
        var rawValue = obx.GetField(5);
        // OBX-6: Units
        var units = obx.GetField(6);
        // OBX-11: Observation Result Status (F=Final, P=Preliminary, C=Corrected)
        var resultStatus = obx.GetField(11);

        var observation = new Observation
        {
            Status = MapObservationStatus(resultStatus),
            Code = new CodeableConcept
            {
                Coding =
                [
                    new Coding
                    {
                        System = codingSystem == "LN" ? "http://loinc.org" : null,
                        Code = code,
                        Display = text
                    }
                ],
                Text = text
            },
            Subject = new ResourceReference($"Patient/{patient.Id}"),
            Value = decimal.TryParse(rawValue, out var numericValue)
                ? new Quantity { Value = numericValue, Unit = units }
                : new FhirString(rawValue)
        };

        return observation;
    }

    private static ObservationStatus MapObservationStatus(string code) => code switch
    {
        "F" => ObservationStatus.Final,
        "P" => ObservationStatus.Preliminary,
        "C" => ObservationStatus.Corrected,
        _ => ObservationStatus.Unknown
    };

    private static string? FormatHl7Date(string hl7Date)
    {
        // HL7 v2 dates are YYYYMMDD[HHMMSS]; FHIR wants YYYY-MM-DD for a birth date.
        if (string.IsNullOrWhiteSpace(hl7Date) || hl7Date.Length < 8)
        {
            return null;
        }

        return $"{hl7Date[..4]}-{hl7Date[4..6]}-{hl7Date[6..8]}";
    }

    private static Hl7ConversionResultDto BuildResult(Hl7Message message, List<Resource> resources)
    {
        return new Hl7ConversionResultDto
        {
            Segments = message.Segments.Select(s => new Hl7SegmentDto
            {
                Name = s.Name,
                Fields = s.RawFields.Skip(1).ToList()
            }).ToList(),
            FhirResourcesJson = resources
                .Select(r => JsonSerializer.Serialize(r, r.GetType(), FhirJsonOptions))
                .ToList()
        };
    }
}
