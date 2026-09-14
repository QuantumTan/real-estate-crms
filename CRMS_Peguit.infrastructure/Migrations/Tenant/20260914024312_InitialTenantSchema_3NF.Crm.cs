using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    public partial class InitialTenantSchema_3NF
    {
        private static void BuildCrmSchema(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedAgentId = table.Column<int>(type: "int", nullable: true),
                    AssignmentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignmentReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignmentReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignmentReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_AssignedAgentId",
                        column: x => x.AssignedAgentId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });


            migrationBuilder.CreateTable(
                name: "BuyerProfiles",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PreferredPropertyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerProfiles", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_BuyerProfiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    LeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Stage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpectedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedAgentId = table.Column<int>(type: "int", nullable: true),
                    AssignmentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignmentReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    AssignmentReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignmentReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConvertedCustomerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.LeadId);
                    table.ForeignKey(
                        name: "FK_Leads_Customers_ConvertedCustomerId",
                        column: x => x.ConvertedCustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Leads_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_AssignedAgentId",
                        column: x => x.AssignedAgentId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });


            migrationBuilder.CreateIndex(
                name: "IX_Customers_AssignedAgentId",
                table: "Customers",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByUserId",
                table: "Customers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_IsDeleted",
                table: "Customers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PersonId",
                table: "Customers",
                column: "PersonId");


            migrationBuilder.CreateIndex(
                name: "IX_Leads_AssignedAgentId",
                table: "Leads",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedCustomerId",
                table: "Leads",
                column: "ConvertedCustomerId",
                unique: true,
                filter: "[ConvertedCustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedByUserId",
                table: "Leads",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_IsDeleted",
                table: "Leads",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_PersonId",
                table: "Leads",
                column: "PersonId");

        }

        private static void DropCrmSchema(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuyerProfiles");


            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "Customers");

        }
    }
}