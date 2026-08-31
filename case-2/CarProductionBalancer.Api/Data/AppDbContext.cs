using CarProductionBalancer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarProductionBalancer.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Planning> Plannings => Set<Planning>();
    public DbSet<PlanningSlot> PlanningSlots => Set<PlanningSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RequestCode harus unik biar ga dobel
        modelBuilder.Entity<Planning>()
            .HasIndex(p => p.RequestCode)
            .IsUnique();

        // Kalau planning dihapus, slot detailnya ikut kehapus
        modelBuilder.Entity<Planning>()
            .HasMany(p => p.Slots)
            .WithOne(s => s.Planning)
            .HasForeignKey(s => s.PlanningId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
