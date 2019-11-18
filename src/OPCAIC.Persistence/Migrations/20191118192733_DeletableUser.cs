using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace OPCAIC.Persistence.Migrations
{
    public partial class DeletableUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_AspNetUsers_AuthorId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_TournamentParticipation_TournamentId_AuthorId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentInvitation_AspNetUsers_UserId",
                table: "TournamentInvitation");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentParticipation_AspNetUsers_UserId",
                table: "TournamentParticipation");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_AspNetUsers_OwnerId",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentParticipation",
                table: "TournamentParticipation");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_TournamentId_AuthorId",
                table: "Submissions");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "TournamentParticipation",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "TournamentParticipation",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "AuthorId",
                table: "Submissions",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "TournamentParticipationId",
                table: "Submissions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentParticipation",
                table: "TournamentParticipation",
                column: "Id");

            // populate the new foreign key
            migrationBuilder.Sql(@"UPDATE public.""Submissions"" sub
    set ""TournamentParticipationId"" = par.""Id""
    From public.""TournamentParticipation"" par
    where sub.""TournamentId"" = par.""TournamentId"" and sub.""AuthorId"" = par.""UserId""");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentParticipation_TournamentId",
                table: "TournamentParticipation",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TournamentId",
                table: "Submissions",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TournamentParticipationId",
                table: "Submissions",
                column: "TournamentParticipationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_AspNetUsers_AuthorId",
                table: "Submissions",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_TournamentParticipation_TournamentParticipation~",
                table: "Submissions",
                column: "TournamentParticipationId",
                principalTable: "TournamentParticipation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentInvitation_AspNetUsers_UserId",
                table: "TournamentInvitation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentParticipation_AspNetUsers_UserId",
                table: "TournamentParticipation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_AspNetUsers_OwnerId",
                table: "Tournaments",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_AspNetUsers_AuthorId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_TournamentParticipation_TournamentParticipation~",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentInvitation_AspNetUsers_UserId",
                table: "TournamentInvitation");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentParticipation_AspNetUsers_UserId",
                table: "TournamentParticipation");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_AspNetUsers_OwnerId",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentParticipation",
                table: "TournamentParticipation");

            migrationBuilder.DropIndex(
                name: "IX_TournamentParticipation_TournamentId",
                table: "TournamentParticipation");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_TournamentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_TournamentParticipationId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TournamentParticipation");

            migrationBuilder.DropColumn(
                name: "TournamentParticipationId",
                table: "Submissions");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "TournamentParticipation",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AuthorId",
                table: "Submissions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentParticipation",
                table: "TournamentParticipation",
                columns: new[] { "TournamentId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_TournamentId_AuthorId",
                table: "Submissions",
                columns: new[] { "TournamentId", "AuthorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_AspNetUsers_AuthorId",
                table: "Submissions",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_TournamentParticipation_TournamentId_AuthorId",
                table: "Submissions",
                columns: new[] { "TournamentId", "AuthorId" },
                principalTable: "TournamentParticipation",
                principalColumns: new[] { "TournamentId", "UserId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentInvitation_AspNetUsers_UserId",
                table: "TournamentInvitation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentParticipation_AspNetUsers_UserId",
                table: "TournamentParticipation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_AspNetUsers_OwnerId",
                table: "Tournaments",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
