using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NursingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCheckoutSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentCheckoutSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    NurseProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderCheckoutSessionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderPaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProviderClientReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CheckoutUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ProviderCallLeaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderCallLeaseExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AmountMinor = table.Column<long>(type: "bigint", nullable: false),
                    IdempotencyKeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    RequestFingerprintHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentCheckoutSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentCheckoutSessions_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentCheckoutSessions_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_NurseProfileId_IdempotencyKeyHash",
                table: "PaymentCheckoutSessions",
                columns: new[] { "NurseProfileId", "IdempotencyKeyHash" },
                unique: true,
                filter: "\"IdempotencyKeyHash\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_NurseProfileId_PaymentOrderId",
                table: "PaymentCheckoutSessions",
                columns: new[] { "NurseProfileId", "PaymentOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_PaymentOrderId",
                table: "PaymentCheckoutSessions",
                column: "PaymentOrderId",
                unique: true,
                filter: "\"Status\" IN ('Created', 'ProviderPending')");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_PaymentOrderId_Status_ExpiresAt",
                table: "PaymentCheckoutSessions",
                columns: new[] { "PaymentOrderId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_ProviderClientReference",
                table: "PaymentCheckoutSessions",
                column: "ProviderClientReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_ProviderName_ProviderCheckoutSessio~",
                table: "PaymentCheckoutSessions",
                columns: new[] { "ProviderName", "ProviderCheckoutSessionId" },
                unique: true,
                filter: "\"ProviderCheckoutSessionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCheckoutSessions_ProviderName_ProviderPaymentIntentId",
                table: "PaymentCheckoutSessions",
                columns: new[] { "ProviderName", "ProviderPaymentIntentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentCheckoutSessions");
        }
    }
}
