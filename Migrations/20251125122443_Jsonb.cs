using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class Jsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    ALTER TABLE characters
                    ALTER COLUMN stat_block
                    SET DATA TYPE jsonb
                    USING stat_block::jsonb;
                "
            );

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
