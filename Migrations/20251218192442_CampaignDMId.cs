using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class CampaignDMId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_users_DungeonMasterId",
                table: "Campaigns");

            migrationBuilder.AlterColumn<string>(
                name: "DungeonMasterId",
                table: "Campaigns",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_users_DungeonMasterId",
                table: "Campaigns",
                column: "DungeonMasterId",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_users_DungeonMasterId",
                table: "Campaigns");

            migrationBuilder.AlterColumn<string>(
                name: "DungeonMasterId",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_users_DungeonMasterId",
                table: "Campaigns",
                column: "DungeonMasterId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
