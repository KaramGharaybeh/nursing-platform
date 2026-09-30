using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparationPackageStage2Persistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrderItems_PaymentProducts_ProductId",
                table: "PaymentOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrderItems_ProductId",
                table: "PaymentOrderItems");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "PaymentOrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "PaymentOrderItems",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "ExamAccessProduct");

            migrationBuilder.Sql("""
                UPDATE "PaymentOrderItems"
                SET "SourceType" = 'ExamAccessProduct', "SourceId" = "ProductId"
                WHERE "SourceId" = '00000000-0000-0000-0000-000000000000'
                """);

            migrationBuilder.CreateTable(
                name: "PackageOrderItemSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageOfferTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PackageOfferSlug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    PackageOfferSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageDefinitionTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PackageDefinitionSlug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    IncludedExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludedExamVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludedExamTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReportingProfilePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceAmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AccessDurationDays = table.Column<int>(type: "integer", nullable: false),
                    OrderCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StudyMaterialVersionIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageOrderItemSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageOrderItemSnapshots_PaymentOrderItems_PaymentOrderIte~",
                        column: x => x.PaymentOrderItemId,
                        principalTable: "PaymentOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackagePurchaseEntitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedOfferSnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludedExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludedExamVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingProfilePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceAmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AccessDurationDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FulfilledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessStartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StudyMaterialVersionIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackagePurchaseEntitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_ExamVersions_IncludedExamVersio~",
                        column: x => x.IncludedExamVersionId,
                        principalTable: "ExamVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_Exams_IncludedExamId",
                        column: x => x.IncludedExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PackageOrderItemSnapshots_Purch~",
                        column: x => x.PurchasedOfferSnapshotId,
                        principalTable: "PackageOrderItemSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PaymentOrderItems_PaymentOrderI~",
                        column: x => x.PaymentOrderItemId,
                        principalTable: "PaymentOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PracticeCollectionVersions_Prac~",
                        column: x => x.PracticeCollectionVersionId,
                        principalTable: "PracticeCollectionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PreparationPackageDefinitions_P~",
                        column: x => x.PreparationPackageDefinitionId,
                        principalTable: "PreparationPackageDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PreparationPackageOffers_Prepar~",
                        column: x => x.PreparationPackageOfferId,
                        principalTable: "PreparationPackageOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_PreparationPackageVersions_Prep~",
                        column: x => x.PreparationPackageVersionId,
                        principalTable: "PreparationPackageVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackagePurchaseEntitlements_ReportingProfilePublications_Re~",
                        column: x => x.ReportingProfilePublicationId,
                        principalTable: "ReportingProfilePublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageBenefitRights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagePurchaseEntitlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    RightType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AccessStartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccessEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageBenefitRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageBenefitRights_PackagePurchaseEntitlements_PackagePur~",
                        column: x => x.PackagePurchaseEntitlementId,
                        principalTable: "PackagePurchaseEntitlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_SourceType_SourceId",
                table: "PaymentOrderItems",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageBenefitRights_PackagePurchaseEntitlementId_RightType",
                table: "PackageBenefitRights",
                columns: new[] { "PackagePurchaseEntitlementId", "RightType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageBenefitRights_PackagePurchaseEntitlementId_RightType~",
                table: "PackageBenefitRights",
                columns: new[] { "PackagePurchaseEntitlementId", "RightType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrderItemSnapshots_PackageDefinitionId_PackageVersio~",
                table: "PackageOrderItemSnapshots",
                columns: new[] { "PackageDefinitionId", "PackageVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrderItemSnapshots_PackageOfferId",
                table: "PackageOrderItemSnapshots",
                column: "PackageOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageOrderItemSnapshots_PaymentOrderItemId",
                table: "PackageOrderItemSnapshots",
                column: "PaymentOrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_IncludedExamId",
                table: "PackagePurchaseEntitlements",
                column: "IncludedExamId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_IncludedExamVersionId",
                table: "PackagePurchaseEntitlements",
                column: "IncludedExamVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_NurseProfileId_PreparationPacka~",
                table: "PackagePurchaseEntitlements",
                columns: new[] { "NurseProfileId", "PreparationPackageDefinitionId" },
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_NurseProfileId_Status_AccessEnd~",
                table: "PackagePurchaseEntitlements",
                columns: new[] { "NurseProfileId", "Status", "AccessEndsAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PaymentOrderId",
                table: "PackagePurchaseEntitlements",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PaymentOrderItemId",
                table: "PackagePurchaseEntitlements",
                column: "PaymentOrderItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PracticeCollectionVersionId",
                table: "PackagePurchaseEntitlements",
                column: "PracticeCollectionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PreparationPackageDefinitionId_~",
                table: "PackagePurchaseEntitlements",
                columns: new[] { "PreparationPackageDefinitionId", "PreparationPackageVersionId" });

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PreparationPackageOfferId",
                table: "PackagePurchaseEntitlements",
                column: "PreparationPackageOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PreparationPackageVersionId",
                table: "PackagePurchaseEntitlements",
                column: "PreparationPackageVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_PurchasedOfferSnapshotId",
                table: "PackagePurchaseEntitlements",
                column: "PurchasedOfferSnapshotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackagePurchaseEntitlements_ReportingProfilePublicationId",
                table: "PackagePurchaseEntitlements",
                column: "ReportingProfilePublicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageBenefitRights");

            migrationBuilder.DropTable(
                name: "PackagePurchaseEntitlements");

            migrationBuilder.DropTable(
                name: "PackageOrderItemSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrderItems_SourceType_SourceId",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "PaymentOrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_ProductId",
                table: "PaymentOrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrderItems_PaymentProducts_ProductId",
                table: "PaymentOrderItems",
                column: "ProductId",
                principalTable: "PaymentProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
