namespace EnvComplianceTracker.Domain.Entities;

public class Facility
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public List<WasteRecord> WasteRecords { get; set; } = new();
    public List<EmissionsRecord> EmissionsRecords { get; set; } = new();
}
