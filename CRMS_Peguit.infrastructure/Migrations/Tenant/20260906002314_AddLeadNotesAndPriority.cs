using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddLeadNotesAndPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Leads",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Leads",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Leads");
        }
    }
}
