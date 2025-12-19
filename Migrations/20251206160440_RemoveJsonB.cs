using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJsonB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "stat_block",
                table: "characters",
                type: "text",
                nullable: true,
                oldClrType: typeof(JsonDocument),
                oldType: "jsonb",
                oldNullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<JsonDocument>(
                name: "stat_block",
                table: "characters",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );
        }
    }
}
