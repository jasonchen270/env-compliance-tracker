using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnvComplianceTracker.Api.Controllers;
using EnvComplianceTracker.Domain.Entities;
using Xunit;

namespace EnvComplianceTracker.Api.Tests;

public class AuditTrailTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public AuditTrailTests(ApiFactory factory) => _factory = factory;

    private async Task<HttpClient> Officer()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new AuthController.LoginRequest("officer", "officer"));
        var body = await resp.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    [Fact]
    public async Task Editing_a_waste_record_creates_audit_entries()
    {
        var client = await Officer();
        var facilities = await client.GetFromJsonAsync<List<Facility>>("/api/facilities");
        var fac = facilities!.First();

        var createResp = await client.PostAsJsonAsync("/api/waste-records", new WasteRecord
        {
            FacilityId = fac.Id, ReportedOn = DateTime.UtcNow,
            WasteType = "Sludge", QuantityKg = 100m, DisposalMethod = "Landfill"
        });
        var created = await createResp.Content.ReadFromJsonAsync<WasteRecord>();

        var putResp = await client.PutAsJsonAsync($"/api/waste-records/{created!.Id}", new WasteRecord
        {
            FacilityId = fac.Id, ReportedOn = created.ReportedOn,
            WasteType = "Sludge", QuantityKg = 200m, DisposalMethod = "Incineration"
        });
        putResp.EnsureSuccessStatusCode();

        var audits = await client.GetFromJsonAsync<List<AuditEntry>>(
            $"/api/audit?entityName=WasteRecord&entityId={created.Id}");

        Assert.Contains(audits!, a => a.Action == "Modified" && a.Username == "officer");
        var modified = audits!.First(a => a.Action == "Modified");
        Assert.Contains("Landfill", modified.Before);
        Assert.Contains("Incineration", modified.After);
    }
}
