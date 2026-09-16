using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddFollowUpsTaskReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskReminders",
                columns: table => new
                {
                    TaskReminderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: false),
                    RelatedCustomerId = table.Column<int>(type: "int", nullable: true),
                    RelatedLeadId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReminders", x => x.TaskReminderId);
                    table.ForeignKey(
                        name: "FK_TaskReminders_Customers_RelatedCustomerId",
                        column: x => x.RelatedCustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaskReminders_Leads_RelatedLeadId",
                        column: x => x.RelatedLeadId,
                        principalTable: "Leads",
                        principalColumn: "LeadId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaskReminders_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_AssignedToUserId",
                table: "TaskReminders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_DueDate",
                table: "TaskReminders",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_IsDeleted",
                table: "TaskReminders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_RelatedCustomerId",
                table: "TaskReminders",
                column: "RelatedCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_RelatedLeadId",
                table: "TaskReminders",
                column: "RelatedLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_Status",
                table: "TaskReminders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskReminders");
        }
    }
}
