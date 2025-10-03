using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedIssuesWithBulkSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BulkSchedules_Users_ProcessedByUserId",
                table: "BulkSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByUserId",
                table: "BulkSchedules",
                type: "character varying(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)");

            migrationBuilder.AddForeignKey(
                name: "FK_BulkSchedules_Users_ProcessedByUserId",
                table: "BulkSchedules",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BulkSchedules_Users_ProcessedByUserId",
                table: "BulkSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByUserId",
                table: "BulkSchedules",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BulkSchedules_Users_ProcessedByUserId",
                table: "BulkSchedules",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
