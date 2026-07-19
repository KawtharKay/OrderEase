using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceToReturnRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DebtReductionAmount",
                table: "ReturnRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "ReturnRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ReturnRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WalletCreditAmount",
                table: "ReturnRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ReturnRequestItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebtReductionAmount",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "WalletCreditAmount",
                table: "ReturnRequests");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ReturnRequestItems");
        }
    }
}
