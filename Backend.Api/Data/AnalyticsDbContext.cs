using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;

namespace Backend.Api.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<SensorAggregate> SensorAggregates { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorAggregate>(entity =>
        {
            entity.HasKey(e => new { e.BucketStart, e.SensorId });
            entity.HasIndex(e => e.BucketStart);
            entity.Property(e => e.BucketStart).HasColumnType("TEXT");
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Timestamp).HasColumnType("TEXT");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=analytics.db");
        }
    }
}
