using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparationPackageStage1CatalogAuthoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PracticeCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreparationPackageDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationPackageDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationPackageDefinitions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationPackageDefinitions_ExamCategories_ExamCategoryId",
                        column: x => x.ExamCategoryId,
                        principalTable: "ExamCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportingProfilePublications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingProfilePublications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingProfilePublications_ExamVersions_ExamVersionId",
                        column: x => x.ExamVersionId,
                        principalTable: "ExamVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportingTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingTopics_ExamCategories_ExamCategoryId",
                        column: x => x.ExamCategoryId,
                        principalTable: "ExamCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudyMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PracticeCollectionVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeCollectionVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeCollectionVersions_PracticeCollections_PracticeColl~",
                        column: x => x.PracticeCollectionId,
                        principalTable: "PracticeCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportingProfileQuestionAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingProfilePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingProfileQuestionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportingProfileQuestionAssignments_ExamQuestions_ExamQuest~",
                        column: x => x.ExamQuestionId,
                        principalTable: "ExamQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportingProfileQuestionAssignments_ReportingProfilePublica~",
                        column: x => x.ReportingProfilePublicationId,
                        principalTable: "ReportingProfilePublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportingProfileQuestionAssignments_ReportingTopics_Reporti~",
                        column: x => x.ReportingTopicId,
                        principalTable: "ReportingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudyMaterialVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    MaterialType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FormattedTextContent = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    FileStorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    VideoUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyMaterialVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyMaterialVersions_StudyMaterials_StudyMaterialId",
                        column: x => x.StudyMaterialId,
                        principalTable: "StudyMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PracticeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ImmediateFeedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeItems_PracticeCollectionVersions_PracticeCollection~",
                        column: x => x.PracticeCollectionVersionId,
                        principalTable: "PracticeCollectionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PracticeItems_ReportingTopics_ReportingTopicId",
                        column: x => x.ReportingTopicId,
                        principalTable: "ReportingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparationPackageVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingProfilePublicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeCollectionVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContentIsolationConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationPackageVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationPackageVersions_ExamVersions_ExamVersionId",
                        column: x => x.ExamVersionId,
                        principalTable: "ExamVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationPackageVersions_PracticeCollectionVersions_Pract~",
                        column: x => x.PracticeCollectionVersionId,
                        principalTable: "PracticeCollectionVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationPackageVersions_PreparationPackageDefinitions_Pr~",
                        column: x => x.PreparationPackageDefinitionId,
                        principalTable: "PreparationPackageDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationPackageVersions_ReportingProfilePublications_Rep~",
                        column: x => x.ReportingProfilePublicationId,
                        principalTable: "ReportingProfilePublications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudyMaterialVersionTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyMaterialVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportingTopicId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyMaterialVersionTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyMaterialVersionTopics_ReportingTopics_ReportingTopicId",
                        column: x => x.ReportingTopicId,
                        principalTable: "ReportingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyMaterialVersionTopics_StudyMaterialVersions_StudyMater~",
                        column: x => x.StudyMaterialVersionId,
                        principalTable: "StudyMaterialVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PracticeAnswerOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PracticeItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OptionText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PracticeAnswerOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PracticeAnswerOptions_PracticeItems_PracticeItemId",
                        column: x => x.PracticeItemId,
                        principalTable: "PracticeItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparationPackageOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PriceAmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AccessDurationDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationPackageOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationPackageOffers_PreparationPackageDefinitions_Prep~",
                        column: x => x.PreparationPackageDefinitionId,
                        principalTable: "PreparationPackageDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationPackageOffers_PreparationPackageVersions_Prepara~",
                        column: x => x.PreparationPackageVersionId,
                        principalTable: "PreparationPackageVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreparationPackageVersionMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationPackageVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudyMaterialVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationPackageVersionMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationPackageVersionMaterials_PreparationPackageVersio~",
                        column: x => x.PreparationPackageVersionId,
                        principalTable: "PreparationPackageVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparationPackageVersionMaterials_StudyMaterialVersions_St~",
                        column: x => x.StudyMaterialVersionId,
                        principalTable: "StudyMaterialVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PracticeAnswerOptions_PracticeItemId_DisplayOrder",
                table: "PracticeAnswerOptions",
                columns: new[] { "PracticeItemId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeCollections_Slug",
                table: "PracticeCollections",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeCollectionVersions_PracticeCollectionId_VersionNumb~",
                table: "PracticeCollectionVersions",
                columns: new[] { "PracticeCollectionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeItems_PracticeCollectionVersionId_DisplayOrder",
                table: "PracticeItems",
                columns: new[] { "PracticeCollectionVersionId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PracticeItems_ReportingTopicId",
                table: "PracticeItems",
                column: "ReportingTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageDefinitions_CountryId_ExamCategoryId_Title",
                table: "PreparationPackageDefinitions",
                columns: new[] { "CountryId", "ExamCategoryId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageDefinitions_ExamCategoryId",
                table: "PreparationPackageDefinitions",
                column: "ExamCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageDefinitions_Slug",
                table: "PreparationPackageDefinitions",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageOffers_PreparationPackageDefinitionId",
                table: "PreparationPackageOffers",
                column: "PreparationPackageDefinitionId",
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageOffers_PreparationPackageVersionId",
                table: "PreparationPackageOffers",
                column: "PreparationPackageVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageOffers_Slug",
                table: "PreparationPackageOffers",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersionMaterials_PreparationPackageVersi~1",
                table: "PreparationPackageVersionMaterials",
                columns: new[] { "PreparationPackageVersionId", "StudyMaterialVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersionMaterials_PreparationPackageVersio~",
                table: "PreparationPackageVersionMaterials",
                columns: new[] { "PreparationPackageVersionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersionMaterials_StudyMaterialVersionId",
                table: "PreparationPackageVersionMaterials",
                column: "StudyMaterialVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersions_ExamVersionId",
                table: "PreparationPackageVersions",
                column: "ExamVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersions_PracticeCollectionVersionId",
                table: "PreparationPackageVersions",
                column: "PracticeCollectionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersions_PreparationPackageDefinitionId_V~",
                table: "PreparationPackageVersions",
                columns: new[] { "PreparationPackageDefinitionId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparationPackageVersions_ReportingProfilePublicationId",
                table: "PreparationPackageVersions",
                column: "ReportingProfilePublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProfilePublications_ExamVersionId_Name",
                table: "ReportingProfilePublications",
                columns: new[] { "ExamVersionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProfileQuestionAssignments_ExamQuestionId",
                table: "ReportingProfileQuestionAssignments",
                column: "ExamQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProfileQuestionAssignments_ReportingProfilePublica~",
                table: "ReportingProfileQuestionAssignments",
                columns: new[] { "ReportingProfilePublicationId", "ExamQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingProfileQuestionAssignments_ReportingTopicId",
                table: "ReportingProfileQuestionAssignments",
                column: "ReportingTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingTopics_ExamCategoryId_Name",
                table: "ReportingTopics",
                columns: new[] { "ExamCategoryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportingTopics_ExamCategoryId_Slug",
                table: "ReportingTopics",
                columns: new[] { "ExamCategoryId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterials_Slug",
                table: "StudyMaterials",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterialVersions_StudyMaterialId_VersionNumber",
                table: "StudyMaterialVersions",
                columns: new[] { "StudyMaterialId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterialVersionTopics_ReportingTopicId",
                table: "StudyMaterialVersionTopics",
                column: "ReportingTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterialVersionTopics_StudyMaterialVersionId_Reporting~",
                table: "StudyMaterialVersionTopics",
                columns: new[] { "StudyMaterialVersionId", "ReportingTopicId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PracticeAnswerOptions");

            migrationBuilder.DropTable(
                name: "PreparationPackageOffers");

            migrationBuilder.DropTable(
                name: "PreparationPackageVersionMaterials");

            migrationBuilder.DropTable(
                name: "ReportingProfileQuestionAssignments");

            migrationBuilder.DropTable(
                name: "StudyMaterialVersionTopics");

            migrationBuilder.DropTable(
                name: "PracticeItems");

            migrationBuilder.DropTable(
                name: "PreparationPackageVersions");

            migrationBuilder.DropTable(
                name: "StudyMaterialVersions");

            migrationBuilder.DropTable(
                name: "ReportingTopics");

            migrationBuilder.DropTable(
                name: "PracticeCollectionVersions");

            migrationBuilder.DropTable(
                name: "PreparationPackageDefinitions");

            migrationBuilder.DropTable(
                name: "ReportingProfilePublications");

            migrationBuilder.DropTable(
                name: "StudyMaterials");

            migrationBuilder.DropTable(
                name: "PracticeCollections");
        }
    }
}
