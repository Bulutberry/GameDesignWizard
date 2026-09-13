using GameDesignWizard.Core.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GameDesignWizard.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CatalogOption> CatalogOptions => Set<CatalogOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var catalogOption = modelBuilder.Entity<CatalogOption>();
        catalogOption.ToTable("CatalogOptions");
        catalogOption.HasKey(option => option.Id);
        catalogOption.Property(option => option.NameEnglish).HasMaxLength(200).IsRequired();
        catalogOption.Property(option => option.NormalizedName).HasMaxLength(200).IsRequired();
        catalogOption.Property(option => option.DescriptionEnglish).HasMaxLength(1000);
        catalogOption.HasIndex(option => new { option.Category, option.NormalizedName }).IsUnique();
        catalogOption.HasIndex(option => new { option.Category, option.SortOrder });
    }
}
