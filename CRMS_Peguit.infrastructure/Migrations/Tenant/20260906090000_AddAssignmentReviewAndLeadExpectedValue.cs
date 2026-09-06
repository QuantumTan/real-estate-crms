using System;
using CRMS_Peguit.infrastructure.data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    [DbContext(typeof(RealEstateDbContext))]
    [Migration("20260906090000_AddAssignmentReviewAndLeadExpectedValue")]
    public partial class AddAssignmentReviewAndLeadExpectedValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedValue",
                table: "Leads",
                type: "decimal(18,2)",
                nullable: true);

            AddAssignmentReviewColumns(migrationBuilder, "Leads");
            AddAssignmentReviewColumns(migrationBuilder, "Customers");
            AddAssignmentReviewColumns(migrationBuilder, "Properties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropAssignmentReviewColumns(migrationBuilder, "Properties");
            DropAssignmentReviewColumns(migrationBuilder, "Customers");
            DropAssignmentReviewColumns(migrationBuilder, "Leads");

            migrationBuilder.DropColumn(
                name: "ExpectedValue",
                table: "Leads");
        }

        private static void AddAssignmentReviewColumns(
            MigrationBuilder migrationBuilder,
            string tableName)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentStatus",
                table: tableName,
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "approved");

            migrationBuilder.AddColumn<int>(
                name: "AssignmentReviewedByUserId",
                table: tableName,
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentReviewedAt",
                table: tableName,
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentReviewNotes",
                table: tableName,
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        private static void DropAssignmentReviewColumns(
            MigrationBuilder migrationBuilder,
            string tableName)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentReviewNotes",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "AssignmentReviewedAt",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "AssignmentReviewedByUserId",
                table: tableName);

            migrationBuilder.DropColumn(
                name: "AssignmentStatus",
                table: tableName);
        }
    }
}
