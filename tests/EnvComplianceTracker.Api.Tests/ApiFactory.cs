using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;

namespace EnvComplianceTracker.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conn = new("Filename=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conn.Open();
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<ComplianceDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<ComplianceDbContext>(o => o.UseSqlite(_conn));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _conn.Dispose();
    }
}
