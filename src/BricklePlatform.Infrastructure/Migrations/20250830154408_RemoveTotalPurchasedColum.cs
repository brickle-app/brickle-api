using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTotalPurchasedColum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tokens_purchased",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.AlterColumn<decimal>(
                name: "risk_level",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "risk_level",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<int>(
                name: "tokens_purchased",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
