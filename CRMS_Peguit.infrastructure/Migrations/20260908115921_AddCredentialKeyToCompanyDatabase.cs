using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCredentialKeyToCompanyDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CompanyDatabases') AND name = 'CredentialKey')
BEGIN
    ALTER TABLE [CompanyDatabases] ADD [CredentialKey] nvarchar(100) NOT NULL DEFAULT '';
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Device') AND NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Devices')
BEGIN
    EXEC sp_rename 'Device', 'Devices';
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Devices')
BEGIN
    CREATE TABLE [Devices] (
        [DeviceId] uniqueidentifier NOT NULL,
        [CompanyId] int NOT NULL,
        [DeviceCode] nvarchar(50) NOT NULL,
        [DeviceName] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Devices] PRIMARY KEY ([DeviceId]),
        CONSTRAINT [FK_Devices_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Companies_CompanyId",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "AssignmentReviewNotes",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AssignmentReviewedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AssignmentReviewedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AssignmentStatus",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CredentialKey",
                table: "CompanyDatabases");

            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "Device");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_DeviceCode",
                table: "Device",
                newName: "IX_Device_DeviceCode");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_CompanyId",
                table: "Device",
                newName: "IX_Device_CompanyId");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Device",
                table: "Device",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Companies_CompanyId",
                table: "Device",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
