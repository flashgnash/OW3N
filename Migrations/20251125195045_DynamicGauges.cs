using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class DynamicGauges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.RenameColumn(
            //     name: "stat_block_message_id",
            //     table: "users",
            //     newName: "StatBlockMessageId");

            // migrationBuilder.RenameColumn(
            //     name: "stat_block_hash",
            //     table: "users",
            //     newName: "StatBlockHash");

            // migrationBuilder.RenameColumn(
            //     name: "stat_block_channel_id",
            //     table: "users",
            //     newName: "StatBlockChannelId");

            // migrationBuilder.RenameColumn(
            //     name: "stat_block",
            //     table: "users",
            //     newName: "StatBlock");

            migrationBuilder.CreateTable(
                name: "Gauge",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Max = table.Column<int>(type: "integer", nullable: false),
                    PlayerCharacterId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gauge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gauge_characters_PlayerCharacterId",
                        column: x => x.PlayerCharacterId,
                        principalTable: "characters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gauge_PlayerCharacterId",
                table: "Gauge",
                column: "PlayerCharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gauge");

            // migrationBuilder.RenameColumn(
            //     name: "StatBlockMessageId",
            //     table: "users",
            //     newName: "stat_block_message_id");

            // migrationBuilder.RenameColumn(
            //     name: "StatBlockHash",
            //     table: "users",
            //     newName: "stat_block_hash");

            // migrationBuilder.RenameColumn(
            //     name: "StatBlockChannelId",
            //     table: "users",
            //     newName: "stat_block_channel_id");

            // migrationBuilder.RenameColumn(
            //     name: "StatBlock",
            //     table: "users",
            //     newName: "stat_block");

            // migrationBuilder.AddColumn<string>(
            //     name: "stat_block_channel_id",
            //     table: "characters",
            //     type: "text",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "stat_block_hash",
            //     table: "characters",
            //     type: "text",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "stat_block_message_id",
            //     table: "characters",
            //     type: "text",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "stat_block_server_id",
            //     table: "characters",
            //     type: "text",
            //     nullable: true);
        }
    }
}
