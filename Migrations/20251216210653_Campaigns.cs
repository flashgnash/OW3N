using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class Campaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "characters",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DungeonMasterId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_users_DungeonMasterId",
                        column: x => x.DungeonMasterId,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_characters_CampaignId",
                table: "characters",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_DungeonMasterId",
                table: "Campaigns",
                column: "DungeonMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_characters_Campaigns_CampaignId",
                table: "characters",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_characters_Campaigns_CampaignId",
                table: "characters");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_characters_CampaignId",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "characters");
        }
    }
}
