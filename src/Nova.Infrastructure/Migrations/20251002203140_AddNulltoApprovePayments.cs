using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNulltoApprovePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "Payments",
                type: "character varying(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "Payments",
                type: "character varying(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldNullable: true);
        }
    }
}
