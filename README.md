# FHIR/HL7 Interop Toolkit

A full-stack .NET learning project covering both halves of healthcare interoperability:
querying a live **FHIR R4** server, and hand-parsing legacy **HL7 v2** messages into FHIR
resources.

## What it does

- **Patient Search & Clinical Timeline** — search patients by name against the public
  [HAPI FHIR R4 test server](https://hapi.fhir.org/baseR4), then view a single patient's
  Encounters, Conditions, Observations, and MedicationRequests in one query using FHIR's
  `_include`/`_revinclude` search. Conditions show their SNOMED CT code, Observations show
  LOINC, and Medications show RxNorm.
- **HL7 v2 → FHIR Converter** — paste a raw HL7 v2 ADT (admit/register) or ORU (lab result)
  message. The backend hand-parses the pipe/component-delimited segments (PID, PV1, OBX) and
  maps them to the equivalent FHIR `Patient`/`Encounter`/`Observation` resources, shown
  side-by-side with the original raw segments.

## Architecture

```
FhirProject.Api/          ASP.NET Core Web API (BFF)
  Services/FhirService.cs         FHIR client (Firely SDK) against hapi.fhir.org
  Services/Hl7V2ParserService.cs  Hand-rolled HL7 v2 segment/field parser + FHIR mapping
  Controllers/                    PatientsController, Hl7Controller

FhirProject.Client/       Blazor WebAssembly frontend
  Pages/PatientSearch.razor
  Pages/PatientTimeline.razor
  Pages/Hl7Converter.razor
  Services/ApiClient.cs           Typed HttpClient wrapping the backend API

FhirProject.Shared/       DTOs shared by both projects (also holds sample HL7v2 messages)

FhirProject.Api.Tests/    xUnit tests for the HL7 v2 parser
```

The Blazor client never talks to the FHIR server directly — it always goes through the
ASP.NET Core API, which avoids CORS issues and keeps the Firely SDK dependency server-side.

## Running it

Requires the .NET 9 SDK.

```bash
# Terminal 1 — backend
cd FhirProject.Api
dotnet run --urls http://localhost:5113

# Terminal 2 — frontend
cd FhirProject.Client
dotnet run --urls http://localhost:5210
```

Then open http://localhost:5210 in a browser. The client reads the backend's base URL from
`FhirProject.Client/wwwroot/appsettings.json` (`ApiBaseUrl`) — update it if you run the API
on a different port.

Run the parser unit tests with:

```bash
dotnet test FhirProject.Api.Tests
```

## Next steps

Not built, but natural follow-on work if you want to extend this further:

- **SMART on FHIR launch** — add an OAuth2/OIDC EHR-launch flow instead of talking to the
  open public test server directly.
- **C-CDA export** — generate a Continuity of Care Document from a patient's timeline data,
  covering the other major clinical-document interoperability standard alongside FHIR/HL7v2.
- **CDS Hooks** — expose a simple clinical decision support hook (e.g. flag a drug-drug
  interaction) triggered from the patient timeline.
