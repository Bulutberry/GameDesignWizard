using GameDesignWizard.Core.Catalog;
using Microsoft.EntityFrameworkCore;

namespace GameDesignWizard.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CatalogOption> CatalogOptions => Set<CatalogOption>();

    public DbSet<GameIdeaEntity> GameIdeas => Set<GameIdeaEntity>();

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

        var gameIdea = modelBuilder.Entity<GameIdeaEntity>();
        gameIdea.ToTable("GameIdeas");
        gameIdea.HasKey(idea => idea.Id);
        gameIdea.Property(idea => idea.NameEnglish).HasMaxLength(200).IsRequired();
        gameIdea.Property(idea => idea.DocumentJson).IsRequired();
        gameIdea.HasIndex(idea => new { idea.PoolGroup, idea.UpdatedAtUtc });
    }
}
