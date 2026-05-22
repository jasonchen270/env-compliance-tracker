namespace EnvComplianceTracker.Domain.Entities;

public class EmissionsRecord
{
    public int Id { get; set; }
    public int FacilityId { get; set; }
    public Facility? Facility { get; set; }
    public DateTime ReportedOn { get; set; }
    public string Pollutant { get; set; } = "";
    public decimal QuantityTonsCO2e { get; set; }
}
