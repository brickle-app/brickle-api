using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxWithholdingToLeasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ReteFuentePct",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReteIcaPct",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReteFuentePct",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "ReteIcaPct",
                schema: "dbo",
                table: "UserLeasingAgreement");
        }
    }
}
