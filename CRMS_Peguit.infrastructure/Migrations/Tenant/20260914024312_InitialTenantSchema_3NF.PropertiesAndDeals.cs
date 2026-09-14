using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    public partial class InitialTenantSchema_3NF
    {
        private static void BuildPropertiesAndDealsSchema(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerCustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ListedByAgentId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignmentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignmentReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignmentReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignmentReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.PropertyId);
                    table.ForeignKey(
                        name: "FK_Properties_Customers_OwnerCustomerId",
                        column: x => x.OwnerCustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Properties_Users_ListedByAgentId",
                        column: x => x.ListedByAgentId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    DealId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpectedCloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentScheme = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReservationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DownPaymentPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CgtPayer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DstPayer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferTaxPayer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationFeePayer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialStipulations = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ContractSignedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.DealId);
                    table.ForeignKey(
                        name: "FK_Deals_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "PropertyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_Users_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DealClauses",
                columns: table => new
                {
                    DealClauseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    ClauseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClauseText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealClauses", x => x.DealClauseId);
                    table.ForeignKey(
                        name: "FK_DealClauses_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "DealId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DealContingencies",
                columns: table => new
                {
                    DealContingencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DealId = table.Column<int>(type: "int", nullable: false),
                    ContingencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSatisfied = table.Column<bool>(type: "bit", nullable: false),
                    SatisfiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealContingencies", x => x.DealContingencyId);
                    table.ForeignKey(
                        name: "FK_DealContingencies_Deals_DealId",
                        column: x => x.DealId,
                        principalTable: "Deals",
                        principalColumn: "DealId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealClauses_DealId",
                table: "DealClauses",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_DealContingencies_DealId",
                table: "DealContingencies",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_AgentId",
                table: "Deals",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CreatedByUserId",
                table: "Deals",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CustomerId",
                table: "Deals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_PropertyId",
                table: "Deals",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CreatedByUserId",
                table: "Properties",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ListedByAgentId",
                table: "Properties",
                column: "ListedByAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerCustomerId",
                table: "Properties",
                column: "OwnerCustomerId");
        }

        private static void DropPropertiesAndDealsSchema(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealClauses");

            migrationBuilder.DropTable(
                name: "DealContingencies");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropTable(
                name: "Properties");
        }
    }
}