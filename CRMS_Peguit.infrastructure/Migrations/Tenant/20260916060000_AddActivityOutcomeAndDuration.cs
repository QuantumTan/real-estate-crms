using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddActivityOutcomeAndDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Activities",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Activities");
        }
    }
}
