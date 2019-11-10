using Microsoft.EntityFrameworkCore.Migrations;

namespace OPCAIC.Persistence.Migrations
{
    public partial class Explicit_LastValidation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_MatchExecutions_LastExecutionId1",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_LastExecutionId1",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LastExecutionId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LastExecutionId1",
                table: "Matches");

            migrationBuilder.AddColumn<long>(
                name: "SubmissionId1",
                table: "SubmissionValidations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MatchId1",
                table: "MatchExecutions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionValidations_SubmissionId1",
                table: "SubmissionValidations",
                column: "SubmissionId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchExecutions_MatchId1",
                table: "MatchExecutions",
                column: "MatchId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchExecutions_Matches_MatchId1",
                table: "MatchExecutions",
                column: "MatchId1",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionValidations_Submissions_SubmissionId1",
                table: "SubmissionValidations",
                column: "SubmissionId1",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchExecutions_Matches_MatchId1",
                table: "MatchExecutions");

            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionValidations_Submissions_SubmissionId1",
                table: "SubmissionValidations");

            migrationBuilder.DropIndex(
                name: "IX_SubmissionValidations_SubmissionId1",
                table: "SubmissionValidations");

            migrationBuilder.DropIndex(
                name: "IX_MatchExecutions_MatchId1",
                table: "MatchExecutions");

            migrationBuilder.DropColumn(
                name: "SubmissionId1",
                table: "SubmissionValidations");

            migrationBuilder.DropColumn(
                name: "MatchId1",
                table: "MatchExecutions");

            migrationBuilder.AddColumn<long>(
                name: "LastExecutionId",
                table: "Matches",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastExecutionId1",
                table: "Matches",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LastExecutionId1",
                table: "Matches",
                column: "LastExecutionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_MatchExecutions_LastExecutionId1",
                table: "Matches",
                column: "LastExecutionId1",
                principalTable: "MatchExecutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
