using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDesignWizard.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameIdeas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameIdeas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NameEnglish = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Stage = table.Column<int>(type: "INTEGER", nullable: false),
                    PoolGroup = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameIdeas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameIdeas_PoolGroup_UpdatedAtUtc",
                table: "GameIdeas",
                columns: new[] { "PoolGroup", "UpdatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameIdeas");
        }
    }
}
