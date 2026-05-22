using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnvComplianceTracker.Api.Controllers;
using EnvComplianceTracker.Domain.Entities;
using Xunit;

namespace EnvComplianceTracker.Api.Tests;

public class AuthAndRolesTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public AuthAndRolesTests(ApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AuthedClient(string user, string pw)
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new AuthController.LoginRequest(user, pw));
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<AuthController.LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    [Fact]
    public async Task Anonymous_cannot_list_facilities()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/facilities");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Viewer_can_list_but_not_create()
    {
        var client = await AuthedClient("viewer", "viewer");
        var listResp = await client.GetAsync("/api/facilities");
        listResp.EnsureSuccessStatusCode();

        var createResp = await client.PostAsJsonAsync("/api/facilities",
            new Facility { Name = "X", Location = "Y" });
        Assert.Equal(HttpStatusCode.Forbidden, createResp.StatusCode);
    }

    [Fact]
    public async Task Officer_can_create_facility()
    {
        var client = await AuthedClient("officer", "officer");
        var resp = await client.PostAsJsonAsync("/api/facilities",
            new Facility { Name = "Plant Gamma", Location = "Detroit, MI" });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Bad_password_returns_401()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new AuthController.LoginRequest("admin", "wrong"));
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
