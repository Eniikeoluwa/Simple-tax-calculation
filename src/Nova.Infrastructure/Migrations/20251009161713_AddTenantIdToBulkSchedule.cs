using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToBulkSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_ApprovedByUserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ApprovedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "BulkSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "BulkSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "BulkSchedules",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BulkSchedules_TenantId",
                table: "BulkSchedules",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_BulkSchedules_Tenants_TenantId",
                table: "BulkSchedules",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BulkSchedules_Tenants_TenantId",
                table: "BulkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_BulkSchedules_TenantId",
                table: "BulkSchedules");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "BulkSchedules");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "BulkSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BulkSchedules");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Payments",
                type: "character varying(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ApprovedByUserId",
                table: "Payments",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_ApprovedByUserId",
                table: "Payments",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
