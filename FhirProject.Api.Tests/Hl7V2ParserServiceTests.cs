using FhirProject.Api.Services;
using FhirProject.Shared;
using Xunit;

namespace FhirProject.Api.Tests;

public class Hl7V2ParserServiceTests
{
    private readonly Hl7V2ParserService _parser = new();

    [Fact]
    public void ParseAdt_ExtractsPatientAndEncounter()
    {
        var result = _parser.ParseAdt(Hl7SampleMessages.SampleAdt);

        Assert.Equal(2, result.FhirResourcesJson.Count);
        Assert.Contains("\"resourceType\": \"Patient\"", result.FhirResourcesJson[0]);
        Assert.Contains("\"family\": \"Doe\"", result.FhirResourcesJson[0]);
        Assert.Contains("\"given\": [\n        \"Jane\"\n      ]", result.FhirResourcesJson[0].Replace("\r\n", "\n"));
        Assert.Contains("\"birthDate\": \"1985-03-12\"", result.FhirResourcesJson[0]);
        Assert.Contains("\"gender\": \"female\"", result.FhirResourcesJson[0]);

        Assert.Contains("\"resourceType\": \"Encounter\"", result.FhirResourcesJson[1]);
        Assert.Contains("\"code\": \"IMP\"", result.FhirResourcesJson[1]);
    }

    [Fact]
    public void ParseAdt_ThrowsWhenPidSegmentMissing()
    {
        const string messageWithoutPid = "MSH|^~\\&|APP|FAC|APP2|FAC2|20260714153000||ADT^A01|MSG1|P|2.5";

        Assert.Throws<InvalidOperationException>(() => _parser.ParseAdt(messageWithoutPid));
    }

    [Fact]
    public void ParseOru_ExtractsPatientAndObservationsWithLoincCodes()
    {
        var result = _parser.ParseOru(Hl7SampleMessages.SampleOru);

        Assert.Equal(4, result.FhirResourcesJson.Count); // 1 patient + 3 OBX segments
        Assert.Contains("\"resourceType\": \"Patient\"", result.FhirResourcesJson[0]);

        var observationsJson = string.Join("\n", result.FhirResourcesJson.Skip(1));
        Assert.Contains("\"resourceType\": \"Observation\"", observationsJson);
        Assert.Contains("\"system\": \"http://loinc.org\"", observationsJson);
        Assert.Contains("\"code\": \"718-7\"", observationsJson);
        Assert.Contains("\"value\": 13.2", observationsJson);
        Assert.Contains("\"unit\": \"g/dL\"", observationsJson);
    }

    [Fact]
    public void ParseOru_ThrowsWhenPidSegmentMissing()
    {
        const string messageWithoutPid = "MSH|^~\\&|LAB|FAC|APP|FAC2|20260714090000||ORU^R01|MSG2|P|2.5";

        Assert.Throws<InvalidOperationException>(() => _parser.ParseOru(messageWithoutPid));
    }
}
