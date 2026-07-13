using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueEffectiveExamAccessGrant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "ExamAccessGrants" AS loser
                USING "ExamAccessGrants" AS keeper
                WHERE loser."ExpiresAt" IS NULL
                    AND keeper."ExpiresAt" IS NULL
                    AND loser."NurseProfileId" = keeper."NurseProfileId"
                    AND loser."ExamId" = keeper."ExamId"
                    AND (loser."GrantedAt", loser."Id") > (keeper."GrantedAt", keeper."Id");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ExamAccessGrants_NurseProfileId_ExamId",
                table: "ExamAccessGrants",
                columns: new[] { "NurseProfileId", "ExamId" },
                unique: true,
                filter: "\"ExpiresAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamAccessGrants_NurseProfileId_ExamId",
                table: "ExamAccessGrants");
        }
    }
}
