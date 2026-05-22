using EnvComplianceTracker.Domain.Entities;
using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Api.Controllers;

[ApiController]
[Route("api/emissions-records")]
[Authorize]
public class EmissionsRecordsController : ControllerBase
{
    private readonly ComplianceDbContext _db;
    public EmissionsRecordsController(ComplianceDbContext db) => _db = db;

    [HttpGet]
    public Task<List<EmissionsRecord>> List([FromQuery] int? facilityId)
    {
        var q = _db.EmissionsRecords.AsNoTracking().AsQueryable();
        if (facilityId is int id) q = q.Where(e => e.FacilityId == id);
        return q.OrderByDescending(e => e.ReportedOn).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
    public async Task<ActionResult<EmissionsRecord>> Create(EmissionsRecord r)
    {
        _db.EmissionsRecords.Add(r);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { id = r.Id }, r);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
    public async Task<IActionResult> Update(int id, EmissionsRecord update)
    {
        var r = await _db.EmissionsRecords.FindAsync(id);
        if (r is null) return NotFound();
        r.Pollutant = update.Pollutant;
        r.QuantityTonsCO2e = update.QuantityTonsCO2e;
        r.ReportedOn = update.ReportedOn;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
