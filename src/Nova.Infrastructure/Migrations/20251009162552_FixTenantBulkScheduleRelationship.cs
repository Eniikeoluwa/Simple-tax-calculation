using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTenantBulkScheduleRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId1",
                table: "BulkSchedules",
                type: "character varying(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BulkSchedules_TenantId1",
                table: "BulkSchedules",
                column: "TenantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BulkSchedules_Tenants_TenantId1",
                table: "BulkSchedules",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BulkSchedules_Tenants_TenantId1",
                table: "BulkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_BulkSchedules_TenantId1",
                table: "BulkSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "BulkSchedules");
        }
    }
}
