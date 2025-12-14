using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    roll_server_id = table.Column<string>(type: "text", nullable: true),
                    stat_block_hash = table.Column<string>(type: "text", nullable: true),
                    stat_block = table.Column<string>(type: "text", nullable: true),
                    stat_block_message_id = table.Column<string>(type: "text", nullable: true),
                    stat_block_channel_id = table.Column<string>(type: "text", nullable: true),
                    spell_block_channel_id = table.Column<string>(type: "text", nullable: true),
                    spell_block_message_id = table.Column<string>(type: "text", nullable: true),
                    spell_block = table.Column<string>(type: "text", nullable: true),
                    spell_block_hash = table.Column<string>(type: "text", nullable: true),
                    mana = table.Column<int>(type: "integer", nullable: true),
                    mana_readout_channel_id = table.Column<string>(type: "text", nullable: true),
                    mana_readout_message_id = table.Column<string>(type: "text", nullable: true),
                    saved_rolls = table.Column<string>(type: "text", nullable: true),
                    stat_block_server_id = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("new_characters_pkey", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    default_roll_channel = table.Column<string>(type: "text", nullable: true),
                    default_roll_server = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("servers_pkey", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: true),
                    count = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    stat_block_hash = table.Column<string>(type: "text", nullable: true),
                    stat_block = table.Column<string>(type: "text", nullable: true),
                    stat_block_message_id = table.Column<string>(type: "text", nullable: true),
                    stat_block_channel_id = table.Column<string>(type: "text", nullable: true),
                    selected_character_id = table.Column<string>(type: "text", nullable: true),
                    selected_character = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "characters");

            migrationBuilder.DropTable(name: "servers");

            migrationBuilder.DropTable(name: "users");
        }
    }
}
