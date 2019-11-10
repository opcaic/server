using Microsoft.EntityFrameworkCore.Migrations;

namespace OPCAIC.Persistence.Migrations
{
    public partial class FavouredChild_Fixup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "LastValidationId",
                table: "Submissions",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastValidationId1",
                table: "Submissions",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastExecutionId",
                table: "Matches",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastExecutionId1",
                table: "Matches",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_LastValidationId1",
                table: "Submissions",
                column: "LastValidationId1");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_SubmissionValidations_LastValidationId1",
                table: "Submissions",
                column: "LastValidationId1",
                principalTable: "SubmissionValidations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_MatchExecutions_LastExecutionId1",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_SubmissionValidations_LastValidationId1",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_LastValidationId1",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Matches_LastExecutionId1",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LastValidationId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastValidationId1",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastExecutionId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "LastExecutionId1",
                table: "Matches");

            migrationBuilder.AddColumn<long>(
                name: "SubmissionId1",
                table: "SubmissionValidations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MatchId1",
                table: "MatchExecutions",
                type: "bigint",
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
    }
}
