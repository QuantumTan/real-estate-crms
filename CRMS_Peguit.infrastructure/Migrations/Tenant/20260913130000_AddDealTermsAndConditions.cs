using System;
using CRMS_Peguit.infrastructure.data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    [DbContext(typeof(RealEstateDbContext))]
    [Migration("20260913130000_AddDealTermsAndConditions")]
    public partial class AddDealTermsAndConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentScheme",
                table: "Deals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservationFee",
                table: "Deals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DownPaymentPercent",
                table: "Deals",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DownPaymentAmount",
                table: "Deals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAmount",
                table: "Deals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CgtPayer",
                table: "Deals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DstPayer",
                table: "Deals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferTaxPayer",
                table: "Deals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationFeePayer",
                table: "Deals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContingenciesJson",
                table: "Deals",
                type: "nvarchar(max)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedClauseIds",
                table: "Deals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialStipulations",
                table: "Deals",
                type: "nvarchar(max)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractSignedDate",
                table: "Deals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ContractSignedDate", table: "Deals");
            migrationBuilder.DropColumn(name: "SpecialStipulations", table: "Deals");
            migrationBuilder.DropColumn(name: "ApprovedClauseIds", table: "Deals");
            migrationBuilder.DropColumn(name: "ContingenciesJson", table: "Deals");
            migrationBuilder.DropColumn(name: "RegistrationFeePayer", table: "Deals");
            migrationBuilder.DropColumn(name: "TransferTaxPayer", table: "Deals");
            migrationBuilder.DropColumn(name: "DstPayer", table: "Deals");
            migrationBuilder.DropColumn(name: "CgtPayer", table: "Deals");
            migrationBuilder.DropColumn(name: "BalanceAmount", table: "Deals");
            migrationBuilder.DropColumn(name: "DownPaymentAmount", table: "Deals");
            migrationBuilder.DropColumn(name: "DownPaymentPercent", table: "Deals");
            migrationBuilder.DropColumn(name: "ReservationFee", table: "Deals");
            migrationBuilder.DropColumn(name: "PaymentScheme", table: "Deals");
        }
    }
}
