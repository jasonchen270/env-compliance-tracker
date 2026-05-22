using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnvComplianceTracker.Api.Controllers;
using EnvComplianceTracker.Domain.Entities;
using Xunit;

namespace EnvComplianceTracker.Api.Tests;

public class ReportsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public ReportsTests(ApiFactory factory) => _factory = factory;

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
    public async Task FacilitySummary_aggregates_waste_and_emissions()
    {
        var client = await Officer();
        var facilities = await client.GetFromJsonAsync<List<Facility>>("/api/facilities");
        var fac = facilities!.First();

        await client.PostAsJsonAsync("/api/waste-records", new WasteRecord
        {
            FacilityId = fac.Id, ReportedOn = DateTime.UtcNow,
            WasteType = "Sludge", QuantityKg = 100m, DisposalMethod = "Incineration"
        });
        await client.PostAsJsonAsync("/api/waste-records", new WasteRecord
        {
            FacilityId = fac.Id, ReportedOn = DateTime.UtcNow,
            WasteType = "Solvent", QuantityKg = 50m, DisposalMethod = "Recycle"
        });
        await client.PostAsJsonAsync("/api/emissions-records", new EmissionsRecord
        {
            FacilityId = fac.Id, ReportedOn = DateTime.UtcNow,
            Pollutant = "CO2", QuantityTonsCO2e = 12.5m
        });

        var summary = await client.GetFromJsonAsync<List<ReportsController.FacilitySummary>>(
            "/api/reports/facility-summary");
        var row = summary!.Single(s => s.FacilityId == fac.Id);
        Assert.Equal(150m, row.TotalWasteKg);
        Assert.Equal(12.5m, row.TotalEmissionsTonsCO2e);
        Assert.Equal(2, row.WasteRecordCount);
        Assert.Equal(1, row.EmissionsRecordCount);
    }
}
