using EnvComplianceTracker.Domain.Entities;
using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Api.Controllers;

[ApiController]
[Route("api/waste-records")]
[Authorize]
public class WasteRecordsController : ControllerBase
{
    private readonly ComplianceDbContext _db;
    public WasteRecordsController(ComplianceDbContext db) => _db = db;

    [HttpGet]
    public Task<List<WasteRecord>> List([FromQuery] int? facilityId)
    {
        var q = _db.WasteRecords.AsNoTracking().AsQueryable();
        if (facilityId is int id) q = q.Where(w => w.FacilityId == id);
        return q.OrderByDescending(w => w.ReportedOn).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
    public async Task<ActionResult<WasteRecord>> Create(WasteRecord r)
    {
        _db.WasteRecords.Add(r);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { id = r.Id }, r);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
    public async Task<IActionResult> Update(int id, WasteRecord update)
    {
        var r = await _db.WasteRecords.FindAsync(id);
        if (r is null) return NotFound();
        r.WasteType = update.WasteType;
        r.QuantityKg = update.QuantityKg;
        r.DisposalMethod = update.DisposalMethod;
        r.ReportedOn = update.ReportedOn;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _db.WasteRecords.FindAsync(id);
        if (r is null) return NotFound();
        _db.WasteRecords.Remove(r);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
