using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class DynamicGauges_Squash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gauge_characters_PlayerCharacterId",
                table: "Gauge"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Gauge", table: "Gauge");

            migrationBuilder.RenameTable(name: "Gauge", newName: "Gauges");

            migrationBuilder.RenameIndex(
                name: "IX_Gauge_PlayerCharacterId",
                table: "Gauges",
                newName: "IX_Gauges_PlayerCharacterId"
            );

            migrationBuilder.AlterColumn<int>(
                name: "PlayerCharacterId",
                table: "Gauges",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Gauges", table: "Gauges", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gauges_characters_PlayerCharacterId",
                table: "Gauges",
                column: "PlayerCharacterId",
                principalTable: "characters",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gauges_characters_PlayerCharacterId",
                table: "Gauges"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Gauges", table: "Gauges");

            migrationBuilder.RenameTable(name: "Gauges", newName: "Gauge");

            migrationBuilder.RenameIndex(
                name: "IX_Gauges_PlayerCharacterId",
                table: "Gauge",
                newName: "IX_Gauge_PlayerCharacterId"
            );

            migrationBuilder.AlterColumn<int>(
                name: "PlayerCharacterId",
                table: "Gauge",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Gauge", table: "Gauge", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gauge_characters_PlayerCharacterId",
                table: "Gauge",
                column: "PlayerCharacterId",
                principalTable: "characters",
                principalColumn: "id"
            );
        }
    }
}
