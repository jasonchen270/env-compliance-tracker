namespace EnvComplianceTracker.Domain.Entities;

public class AuditEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Username { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Before { get; set; }
    public string? After { get; set; }
}
