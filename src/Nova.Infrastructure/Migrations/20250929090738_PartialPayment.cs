using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nova.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartialPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFinalPayment",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPartialPayment",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalInvoiceAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ParentPaymentId",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAmount",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmountPaid",
                table: "Payments",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ParentPaymentId",
                table: "Payments",
                column: "ParentPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Payments_ParentPaymentId",
                table: "Payments",
                column: "ParentPaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Payments_ParentPaymentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ParentPaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsFinalPayment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsPartialPayment",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OriginalInvoiceAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ParentPaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TotalAmountPaid",
                table: "Payments");
        }
    }
}
