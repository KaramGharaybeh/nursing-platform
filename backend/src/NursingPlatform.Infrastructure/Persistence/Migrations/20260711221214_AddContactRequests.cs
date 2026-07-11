using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContactRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployerOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CandidateHeadlineSnapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    CandidateLicenseCountryNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CandidateCurrentCountryNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EmployerOrganizationNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobTitleSnapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    DepartmentSnapshot = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactRequests_EmployerOrganizations_EmployerOrganizationId",
                        column: x => x.EmployerOrganizationId,
                        principalTable: "EmployerOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactRequests_EmployerProfiles_EmployerProfileId",
                        column: x => x.EmployerProfileId,
                        principalTable: "EmployerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactRequests_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_EmployerOrganizationId",
                table: "ContactRequests",
                column: "EmployerOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_EmployerProfileId_CreatedAt_Id",
                table: "ContactRequests",
                columns: new[] { "EmployerProfileId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_EmployerProfileId_NurseProfileId",
                table: "ContactRequests",
                columns: new[] { "EmployerProfileId", "NurseProfileId" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_NurseProfileId_CreatedAt_Id",
                table: "ContactRequests",
                columns: new[] { "NurseProfileId", "CreatedAt", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactRequests");
        }
    }
}
