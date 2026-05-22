using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ComplianceDbContext _db;
    public ReportsController(ComplianceDbContext db) => _db = db;

    public record FacilitySummary(int FacilityId, string FacilityName,
        decimal TotalWasteKg, decimal TotalEmissionsTonsCO2e,
        int WasteRecordCount, int EmissionsRecordCount);

    [HttpGet("facility-summary")]
    public async Task<ActionResult<List<FacilitySummary>>> GetFacilitySummary(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var facilities = await _db.Facilities.AsNoTracking().ToListAsync();
        var waste = _db.WasteRecords.AsNoTracking().AsQueryable();
        var emissions = _db.EmissionsRecords.AsNoTracking().AsQueryable();
        if (from is { } f) { waste = waste.Where(w => w.ReportedOn >= f); emissions = emissions.Where(e => e.ReportedOn >= f); }
        if (to is { } t) { waste = waste.Where(w => w.ReportedOn <= t); emissions = emissions.Where(e => e.ReportedOn <= t); }

        var wasteAgg = await waste.GroupBy(w => w.FacilityId)
            .Select(g => new { FacilityId = g.Key, Total = g.Sum(x => x.QuantityKg), Count = g.Count() })
            .ToListAsync();
        var emAgg = await emissions.GroupBy(e => e.FacilityId)
            .Select(g => new { FacilityId = g.Key, Total = g.Sum(x => x.QuantityTonsCO2e), Count = g.Count() })
            .ToListAsync();

        return facilities.Select(fac =>
        {
            var w = wasteAgg.FirstOrDefault(x => x.FacilityId == fac.Id);
            var e = emAgg.FirstOrDefault(x => x.FacilityId == fac.Id);
            return new FacilitySummary(fac.Id, fac.Name,
                w?.Total ?? 0m, e?.Total ?? 0m, w?.Count ?? 0, e?.Count ?? 0);
        }).ToList();
    }
}
