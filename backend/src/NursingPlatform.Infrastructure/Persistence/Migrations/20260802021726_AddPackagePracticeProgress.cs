using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagePracticeProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackagePracticeProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagePurchaseEntitlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedPracticeAnswerOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    LastAnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagePracticeProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagePracticeProgresses_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePracticeProgresses_PackagePurchaseEntitlements_Packa~",
                        column: x => x.PackagePurchaseEntitlementId,
                        principalTable: "PackagePurchaseEntitlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePracticeProgresses_PracticeAnswerOptions_SelectedPra~",
                        column: x => x.SelectedPracticeAnswerOptionId,
                        principalTable: "PracticeAnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePracticeProgresses_PracticeCollectionVersions_Practi~",
                        column: x => x.PracticeCollectionVersionId,
                        principalTable: "PracticeCollectionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePracticeProgresses_PracticeItems_PracticeItemId",
                        column: x => x.PracticeItemId,
                        principalTable: "PracticeItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackagePracticeProgresses_NurseProfileId_PackagePurchaseEnt~",
                table: "PackagePracticeProgresses",
                columns: new[] { "NurseProfileId", "PackagePurchaseEntitlementId", "PracticeCollectionVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PackagePracticeProgresses_PackagePurchaseEntitlementId_Prac~",
                table: "PackagePracticeProgresses",
                columns: new[] { "PackagePurchaseEntitlementId", "PracticeItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagePracticeProgresses_PracticeCollectionVersionId",
                table: "PackagePracticeProgresses",
                column: "PracticeCollectionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePracticeProgresses_PracticeItemId",
                table: "PackagePracticeProgresses",
                column: "PracticeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePracticeProgresses_SelectedPracticeAnswerOptionId",
                table: "PackagePracticeProgresses",
                column: "SelectedPracticeAnswerOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackagePracticeProgresses");
        }
    }
}
