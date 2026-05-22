using EnvComplianceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Infrastructure;

public static class Seed
{
    public static async Task RunAsync(ComplianceDbContext db)
    {
        await db.Database.MigrateAsync();
        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new AppUser { Username = "admin", PasswordHash = PasswordHasher.Hash("admin"), Role = Roles.Admin },
                new AppUser { Username = "officer", PasswordHash = PasswordHasher.Hash("officer"), Role = Roles.ComplianceOfficer },
                new AppUser { Username = "viewer", PasswordHash = PasswordHasher.Hash("viewer"), Role = Roles.Viewer }
            );
            await db.SaveChangesAsync();
        }
        if (!await db.Facilities.AnyAsync())
        {
            db.Facilities.AddRange(
                new Facility { Name = "Plant Alpha", Location = "Houston, TX" },
                new Facility { Name = "Plant Beta", Location = "Pittsburgh, PA" }
            );
            await db.SaveChangesAsync();
        }
    }
}
