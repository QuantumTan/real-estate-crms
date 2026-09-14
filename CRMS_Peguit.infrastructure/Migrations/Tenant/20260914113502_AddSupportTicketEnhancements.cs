using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddSupportTicketEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SupportTickets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SupportTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "SupportTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstRespondedAt",
                table: "SupportTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SupportTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "SupportTickets",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TicketComments",
                columns: table => new
                {
                    TicketCommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CommentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketComments", x => x.TicketCommentId);
                    table.ForeignKey(
                        name: "FK_TicketComments_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "TicketId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketComments_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_IsDeleted",
                table: "SupportTickets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TicketNumber",
                table: "SupportTickets",
                column: "TicketNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_AuthorUserId",
                table: "TicketComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_TicketId",
                table: "TicketComments",
                column: "TicketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_IsDeleted",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_TicketNumber",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "FirstRespondedAt",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "SupportTickets");
        }
    }
}
