namespace EnvComplianceTracker.Domain.Entities;

public class WasteRecord
{
    public int Id { get; set; }
    public int FacilityId { get; set; }
    public Facility? Facility { get; set; }
    public DateTime ReportedOn { get; set; }
    public string WasteType { get; set; } = "";
    public decimal QuantityKg { get; set; }
    public string DisposalMethod { get; set; } = "";
}
