using Microsoft.EntityFrameworkCore.Migrations;

namespace OPCAIC.Persistence.Migrations
{
    public partial class Fix_Cascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_Tournaments_TournamentId",
                table: "MenuItem");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_Tournaments_TournamentId",
                table: "MenuItem",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItem_Tournaments_TournamentId",
                table: "MenuItem");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItem_Tournaments_TournamentId",
                table: "MenuItem",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
