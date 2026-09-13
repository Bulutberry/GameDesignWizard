using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDesignWizard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    NameEnglish = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DescriptionEnglish = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ParentOptionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlatformPoolGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogOptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptions_Category_NormalizedName",
                table: "CatalogOptions",
                columns: new[] { "Category", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogOptions_Category_SortOrder",
                table: "CatalogOptions",
                columns: new[] { "Category", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogOptions");
        }
    }
}
