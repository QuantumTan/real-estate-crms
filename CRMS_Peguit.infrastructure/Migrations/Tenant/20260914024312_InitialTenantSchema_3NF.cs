using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMS_Peguit.infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class InitialTenantSchema_3NF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            BuildIdentitySchema(migrationBuilder);
            BuildCrmSchema(migrationBuilder);
            BuildPropertiesAndDealsSchema(migrationBuilder);
            BuildOperationsSchema(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropOperationsSchema(migrationBuilder);
            DropPropertiesAndDealsSchema(migrationBuilder);
            DropCrmSchema(migrationBuilder);
            DropIdentitySchema(migrationBuilder);
        }
    }
}