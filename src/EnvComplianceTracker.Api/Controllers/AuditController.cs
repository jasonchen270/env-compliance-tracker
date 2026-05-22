using EnvComplianceTracker.Domain.Entities;
using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
public class AuditController : ControllerBase
{
    private readonly ComplianceDbContext _db;
    public AuditController(ComplianceDbContext db) => _db = db;

    [HttpGet]
    public Task<List<AuditEntry>> List([FromQuery] string? entityName, [FromQuery] string? entityId)
    {
        var q = _db.AuditEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(entityName)) q = q.Where(a => a.EntityName == entityName);
        if (!string.IsNullOrEmpty(entityId)) q = q.Where(a => a.EntityId == entityId);
        return q.OrderByDescending(a => a.Timestamp).Take(500).ToListAsync();
    }
}
