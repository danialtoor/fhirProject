namespace FhirProject.Shared;

// Sample raw HL7 v2 messages used to seed the "load sample" button in the
// HL7 converter UI, and as fixtures for the parser's unit tests.
public static class Hl7SampleMessages
{
    public const string SampleAdt =
        "MSH|^~\\&|REGISTRATION|HOSPITAL|EHR|HOSPITAL|20260714153000||ADT^A01|MSG00001|P|2.5\r" +
        "EVN|A01|20260714153000\r" +
        "PID|1||445566^^^MRN^MR||Doe^Jane^A||19850312|F|||742 Evergreen Terrace^^Springfield^IL^62704||555-0142\r" +
        "PV1|1|I|W389^12^1^HOSP||||1234^Smith^John^^^Dr|||MED||||||||V998877";

    public const string SampleOru =
        "MSH|^~\\&|LAB|HOSPITAL|EHR|HOSPITAL|20260714090000||ORU^R01|MSG00002|P|2.5\r" +
        "PID|1||445566^^^MRN^MR||Doe^Jane^A||19850312|F\r" +
        "OBR|1|||CBC^Complete Blood Count^L\r" +
        "OBX|1|NM|718-7^Hemoglobin^LN||13.2|g/dL|12.0-15.5|N|||F\r" +
        "OBX|2|NM|4544-3^Hematocrit^LN||39|%|36-46|N|||F\r" +
        "OBX|3|NM|789-8^Red Blood Cell Count^LN||4.5|10*6/uL|4.2-5.4|N|||F";
}
