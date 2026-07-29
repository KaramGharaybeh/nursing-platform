using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSessionSourceAndPackageProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedAt",
                table: "PackageBenefitRights",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "ExamSessions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "ExamSessions"
                SET "Source" = 'Legacy'
                WHERE "Source" IS NULL
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "ExamSessions",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ExamSessionProvenances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagePurchaseEntitlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageBenefitRightId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    PackageAccessStartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PackageAccessEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSessionProvenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_ExamSessions_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalTable: "ExamSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_ExamVersions_IncludedExamVersionId",
                        column: x => x.IncludedExamVersionId,
                        principalTable: "ExamVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_Exams_IncludedExamId",
                        column: x => x.IncludedExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PackageBenefitRights_PackageBenefitR~",
                        column: x => x.PackageBenefitRightId,
                        principalTable: "PackageBenefitRights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PackageOrderItemSnapshots_PackageOrd~",
                        column: x => x.PackageOrderItemSnapshotId,
                        principalTable: "PackageOrderItemSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PackagePurchaseEntitlements_PackageP~",
                        column: x => x.PackagePurchaseEntitlementId,
                        principalTable: "PackagePurchaseEntitlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PaymentOrderItems_PaymentOrderItemId",
                        column: x => x.PaymentOrderItemId,
                        principalTable: "PaymentOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PracticeCollectionVersions_PracticeC~",
                        column: x => x.PracticeCollectionVersionId,
                        principalTable: "PracticeCollectionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PreparationPackageDefinitions_Prepar~",
                        column: x => x.PreparationPackageDefinitionId,
                        principalTable: "PreparationPackageDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PreparationPackageOffers_Preparation~",
                        column: x => x.PreparationPackageOfferId,
                        principalTable: "PreparationPackageOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_PreparationPackageVersions_Preparati~",
                        column: x => x.PreparationPackageVersionId,
                        principalTable: "PreparationPackageVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessionProvenances_ReportingProfilePublications_Reporti~",
                        column: x => x.ReportingProfilePublicationId,
                        principalTable: "ReportingProfilePublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_ExamSessionId",
                table: "ExamSessionProvenances",
                column: "ExamSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_IncludedExamId",
                table: "ExamSessionProvenances",
                column: "IncludedExamId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_IncludedExamVersionId",
                table: "ExamSessionProvenances",
                column: "IncludedExamVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PackageBenefitRightId",
                table: "ExamSessionProvenances",
                column: "PackageBenefitRightId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PackageOrderItemSnapshotId",
                table: "ExamSessionProvenances",
                column: "PackageOrderItemSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PackagePurchaseEntitlementId",
                table: "ExamSessionProvenances",
                column: "PackagePurchaseEntitlementId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PaymentOrderId",
                table: "ExamSessionProvenances",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PaymentOrderItemId",
                table: "ExamSessionProvenances",
                column: "PaymentOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PracticeCollectionVersionId",
                table: "ExamSessionProvenances",
                column: "PracticeCollectionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PreparationPackageDefinitionId",
                table: "ExamSessionProvenances",
                column: "PreparationPackageDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PreparationPackageOfferId",
                table: "ExamSessionProvenances",
                column: "PreparationPackageOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_PreparationPackageVersionId",
                table: "ExamSessionProvenances",
                column: "PreparationPackageVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessionProvenances_ReportingProfilePublicationId",
                table: "ExamSessionProvenances",
                column: "ReportingProfilePublicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSessionProvenances");

            migrationBuilder.DropColumn(
                name: "ConsumedAt",
                table: "PackageBenefitRights");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "ExamSessions");
        }
    }
}
