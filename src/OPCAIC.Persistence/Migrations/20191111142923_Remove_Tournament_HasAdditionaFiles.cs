using Microsoft.EntityFrameworkCore.Migrations;

namespace OPCAIC.Persistence.Migrations
{
    public partial class Remove_Tournament_HasAdditionaFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAdditionalFiles",
                table: "Tournaments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAdditionalFiles",
                table: "Tournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
