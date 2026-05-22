using EnvComplianceTracker.Domain.Entities;
using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Api.Controllers;

[ApiController]
[Route("api/facilities")]
[Authorize]
public class FacilitiesController : ControllerBase
{
    private readonly ComplianceDbContext _db;
    public FacilitiesController(ComplianceDbContext db) => _db = db;

    [HttpGet]
    public Task<List<Facility>> List() => _db.Facilities.AsNoTracking().ToListAsync();

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
    public async Task<ActionResult<Facility>> Create(Facility f)
    {
        _db.Facilities.Add(f);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { id = f.Id }, f);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ComplianceOfficer}")]
    public async Task<IActionResult> Update(int id, Facility update)
    {
        var f = await _db.Facilities.FindAsync(id);
        if (f is null) return NotFound();
        f.Name = update.Name;
        f.Location = update.Location;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
