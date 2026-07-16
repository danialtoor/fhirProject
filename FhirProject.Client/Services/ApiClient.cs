using System.Net.Http.Json;
using FhirProject.Shared.Dtos;

namespace FhirProject.Client.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PatientSummaryDto>> SearchPatientsAsync(string name)
    {
        var results = await _httpClient.GetFromJsonAsync<List<PatientSummaryDto>>(
            $"api/patients/search?name={Uri.EscapeDataString(name)}");
        return results ?? [];
    }

    public async Task<PatientTimelineDto?> GetPatientTimelineAsync(string patientId)
    {
        var response = await _httpClient.GetAsync($"api/patients/{Uri.EscapeDataString(patientId)}/timeline");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorText)
                ? $"Request failed with status {(int)response.StatusCode}."
                : errorText);
        }

        return await response.Content.ReadFromJsonAsync<PatientTimelineDto>();
    }

    public Task<Hl7ConversionResultDto> ParseAdtAsync(string rawMessage) =>
        PostForConversionAsync("api/hl7/adt", rawMessage);

    public Task<Hl7ConversionResultDto> ParseOruAsync(string rawMessage) =>
        PostForConversionAsync("api/hl7/oru", rawMessage);

    private async Task<Hl7ConversionResultDto> PostForConversionAsync(string url, string rawMessage)
    {
        var response = await _httpClient.PostAsJsonAsync(url, new Hl7RawMessageRequest { RawMessage = rawMessage });
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(errorText)
                ? $"Request failed with status {(int)response.StatusCode}."
                : errorText);
        }

        return await response.Content.ReadFromJsonAsync<Hl7ConversionResultDto>()
               ?? throw new InvalidOperationException("The server returned an empty response.");
    }
}
