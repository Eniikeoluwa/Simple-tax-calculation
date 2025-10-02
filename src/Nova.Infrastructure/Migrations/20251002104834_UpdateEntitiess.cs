using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntitiess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BulkSchedules_BulkScheduleId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "BulkScheduleId",
                table: "Payments",
                type: "character varying(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BulkSchedules_BulkScheduleId",
                table: "Payments",
                column: "BulkScheduleId",
                principalTable: "BulkSchedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BulkSchedules_BulkScheduleId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "BulkScheduleId",
                table: "Payments",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BulkSchedules_BulkScheduleId",
                table: "Payments",
                column: "BulkScheduleId",
                principalTable: "BulkSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
