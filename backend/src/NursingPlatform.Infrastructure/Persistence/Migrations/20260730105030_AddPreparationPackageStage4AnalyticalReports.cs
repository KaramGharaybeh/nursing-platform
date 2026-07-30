using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparationPackageStage4AnalyticalReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageAnalyticalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamSessionProvenanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagePurchaseEntitlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageOrderItemSnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludedExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludedExamVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingProfilePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinalizedSessionStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    MaxScore = table.Column<int>(type: "integer", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    QuestionCount = table.Column<int>(type: "integer", nullable: false),
                    PackageAccessStartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PackageAccessEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageAnalyticalReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReports_ExamSessionProvenances_ExamSession~",
                        column: x => x.ExamSessionProvenanceId,
                        principalTable: "ExamSessionProvenances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReports_ExamSessions_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalTable: "ExamSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReports_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReports_PackagePurchaseEntitlements_Packag~",
                        column: x => x.PackagePurchaseEntitlementId,
                        principalTable: "PackagePurchaseEntitlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageAnalyticalReportGuidanceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageAnalyticalReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SourceVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TitleSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceMetadataSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageAnalyticalReportGuidanceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReportGuidanceItems_PackageAnalyticalRepor~",
                        column: x => x.PackageAnalyticalReportId,
                        principalTable: "PackageAnalyticalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReportGuidanceItems_ReportingTopics_Report~",
                        column: x => x.ReportingTopicId,
                        principalTable: "ReportingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageAnalyticalReportTopicResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageAnalyticalReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TopicDescriptionSnapshot = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ScoredQuestionCount = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    EarnedPoints = table.Column<int>(type: "integer", nullable: false),
                    AvailablePoints = table.Column<int>(type: "integer", nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageAnalyticalReportTopicResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReportTopicResults_PackageAnalyticalReport~",
                        column: x => x.PackageAnalyticalReportId,
                        principalTable: "PackageAnalyticalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageAnalyticalReportTopicResults_ReportingTopics_Reporti~",
                        column: x => x.ReportingTopicId,
                        principalTable: "ReportingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReportGuidanceItems_PackageAnalyticalRepo~1",
                table: "PackageAnalyticalReportGuidanceItems",
                columns: new[] { "PackageAnalyticalReportId", "ReportingTopicId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReportGuidanceItems_PackageAnalyticalRepor~",
                table: "PackageAnalyticalReportGuidanceItems",
                columns: new[] { "PackageAnalyticalReportId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReportGuidanceItems_ReportingTopicId",
                table: "PackageAnalyticalReportGuidanceItems",
                column: "ReportingTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReports_ExamSessionId",
                table: "PackageAnalyticalReports",
                column: "ExamSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReports_ExamSessionProvenanceId",
                table: "PackageAnalyticalReports",
                column: "ExamSessionProvenanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReports_NurseProfileId_PackagePurchaseEnti~",
                table: "PackageAnalyticalReports",
                columns: new[] { "NurseProfileId", "PackagePurchaseEntitlementId", "ExamSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReports_PackagePurchaseEntitlementId",
                table: "PackageAnalyticalReports",
                column: "PackagePurchaseEntitlementId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReportTopicResults_PackageAnalyticalRepor~1",
                table: "PackageAnalyticalReportTopicResults",
                columns: new[] { "PackageAnalyticalReportId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReportTopicResults_PackageAnalyticalReport~",
                table: "PackageAnalyticalReportTopicResults",
                columns: new[] { "PackageAnalyticalReportId", "ReportingTopicId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageAnalyticalReportTopicResults_ReportingTopicId",
                table: "PackageAnalyticalReportTopicResults",
                column: "ReportingTopicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageAnalyticalReportGuidanceItems");

            migrationBuilder.DropTable(
                name: "PackageAnalyticalReportTopicResults");

            migrationBuilder.DropTable(
                name: "PackageAnalyticalReports");
        }
    }
}
