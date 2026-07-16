namespace FhirProject.Api.Services;

// Minimal hand-rolled HL7 v2 pipe/component parser.
// Fields[0] is the segment name for every segment except MSH, where the field
// separator character ("|") itself is MSH-1 and is consumed by the split below,
// shifting every subsequent MSH field index down by one (Fields[1] == MSH-2, etc).
public class Hl7Segment
{
    public string Name { get; }
    private readonly string[] _fields;

    public Hl7Segment(string rawSegment)
    {
        _fields = rawSegment.Split('|');
        Name = _fields[0];
    }

    public IReadOnlyList<string> RawFields => _fields;

    public string GetField(int fieldNumber)
    {
        if (Name == "MSH")
        {
            if (fieldNumber == 1)
            {
                return "|";
            }

            var mshIndex = fieldNumber - 1;
            return mshIndex < _fields.Length ? _fields[mshIndex] : string.Empty;
        }

        return fieldNumber < _fields.Length ? _fields[fieldNumber] : string.Empty;
    }

    public string GetComponent(int fieldNumber, int componentNumber)
    {
        var components = GetField(fieldNumber).Split('^');
        var index = componentNumber - 1;
        return index >= 0 && index < components.Length ? components[index] : string.Empty;
    }
}

public class Hl7Message
{
    public List<Hl7Segment> Segments { get; }

    public Hl7Message(string rawMessage)
    {
        Segments = rawMessage
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => new Hl7Segment(line))
            .ToList();
    }

    public Hl7Segment? GetSegment(string name) => Segments.FirstOrDefault(s => s.Name == name);

    public IEnumerable<Hl7Segment> GetSegments(string name) => Segments.Where(s => s.Name == name);
}
