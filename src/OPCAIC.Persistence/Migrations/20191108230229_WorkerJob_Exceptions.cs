using Microsoft.EntityFrameworkCore.Migrations;

namespace OPCAIC.Persistence.Migrations
{
    public partial class WorkerJob_Exceptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Exception",
                table: "SubmissionValidations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Exception",
                table: "MatchExecutions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exception",
                table: "SubmissionValidations");

            migrationBuilder.DropColumn(
                name: "Exception",
                table: "MatchExecutions");
        }
    }
}
