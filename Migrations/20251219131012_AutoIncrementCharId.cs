using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordis.Migrations
{
    public partial class AutoIncrementCharId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- create sequence if not exists
                CREATE SEQUENCE IF NOT EXISTS characters_id_seq;

                -- attach sequence to id column
                ALTER TABLE ""characters""
                ALTER COLUMN ""id"" SET DEFAULT nextval('characters_id_seq');

                -- set sequence to max existing id or 1 if table empty
                SELECT setval(
                    'characters_id_seq',
                    GREATEST((SELECT MAX(""id"") FROM ""characters""), 1)
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""characters"" ALTER COLUMN ""id"" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS characters_id_seq;
            ");
        }
    }
}
