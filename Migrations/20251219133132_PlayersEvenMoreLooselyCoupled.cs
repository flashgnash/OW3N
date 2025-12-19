using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class PlayersEvenMoreLooselyCoupled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_characters_Campaigns_CampaignId",
                table: "characters");

            migrationBuilder.AddForeignKey(
                name: "FK_characters_Campaigns_CampaignId",
                table: "characters",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_characters_Campaigns_CampaignId",
                table: "characters");

            migrationBuilder.AddForeignKey(
                name: "FK_characters_Campaigns_CampaignId",
                table: "characters",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id");
        }
    }
}
