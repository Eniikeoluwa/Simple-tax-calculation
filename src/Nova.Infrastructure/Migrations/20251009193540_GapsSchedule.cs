using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GapsSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "GapsSchedules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "GapsSchedules",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessedByUserId",
                table: "GapsSchedules",
                type: "character varying(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "GapsSchedules",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GapsSchedules_CreatedByUserId",
                table: "GapsSchedules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GapsSchedules_ProcessedByUserId",
                table: "GapsSchedules",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GapsSchedules_TenantId",
                table: "GapsSchedules",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_GapsSchedules_Tenants_TenantId",
                table: "GapsSchedules",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GapsSchedules_Users_CreatedByUserId",
                table: "GapsSchedules",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GapsSchedules_Users_ProcessedByUserId",
                table: "GapsSchedules",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GapsSchedules_Tenants_TenantId",
                table: "GapsSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_GapsSchedules_Users_CreatedByUserId",
                table: "GapsSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_GapsSchedules_Users_ProcessedByUserId",
                table: "GapsSchedules");

            migrationBuilder.DropIndex(
                name: "IX_GapsSchedules_CreatedByUserId",
                table: "GapsSchedules");

            migrationBuilder.DropIndex(
                name: "IX_GapsSchedules_ProcessedByUserId",
                table: "GapsSchedules");

            migrationBuilder.DropIndex(
                name: "IX_GapsSchedules_TenantId",
                table: "GapsSchedules");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "GapsSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "GapsSchedules");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "GapsSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GapsSchedules");
        }
    }
}
