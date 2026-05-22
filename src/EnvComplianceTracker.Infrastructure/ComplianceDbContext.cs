using System.Text.Json;
using EnvComplianceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EnvComplianceTracker.Infrastructure;

public class ComplianceDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;

    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<WasteRecord> WasteRecords => Set<WasteRecord>();
    public DbSet<EmissionsRecord> EmissionsRecords => Set<EmissionsRecord>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
        b.Entity<WasteRecord>().Property(w => w.QuantityKg).HasPrecision(18, 3);
        b.Entity<EmissionsRecord>().Property(e => e.QuantityTonsCO2e).HasPrecision(18, 3);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var pending = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditEntry &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(e => new { Entry = e, Snap = Snapshot(e), Action = e.State.ToString() })
            .ToList();

        var result = await base.SaveChangesAsync(ct);

        if (pending.Count > 0)
        {
            foreach (var p in pending)
            {
                AuditEntries.Add(new AuditEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Username = _currentUser.Username ?? "system",
                    EntityName = p.Entry.Entity.GetType().Name,
                    EntityId = KeyOf(p.Entry),
                    Action = p.Action,
                    Before = p.Snap.before,
                    After = p.Snap.after,
                });
            }
            await base.SaveChangesAsync(ct);
        }
        return result;
    }

    private static (string? before, string? after) Snapshot(EntityEntry entry)
    {
        var props = entry.Metadata.GetProperties();
        string Serialize(Func<IProperty, object?> reader)
            => JsonSerializer.Serialize(props.ToDictionary(p => p.Name, p => reader(p)));

        return entry.State switch
        {
            EntityState.Added => (null, Serialize(p => entry.Property(p.Name).CurrentValue)),
            EntityState.Deleted => (Serialize(p => entry.Property(p.Name).OriginalValue), null),
            _ => (Serialize(p => entry.Property(p.Name).OriginalValue),
                  Serialize(p => entry.Property(p.Name).CurrentValue)),
        };
    }

    private static string KeyOf(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return "";
        return string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? ""));
    }
}

public interface ICurrentUser
{
    string? Username { get; }
}

public class SystemUser : ICurrentUser
{
    public string? Username => "system";
}
