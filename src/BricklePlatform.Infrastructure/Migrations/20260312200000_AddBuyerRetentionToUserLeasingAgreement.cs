using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    [Migration("20260312200000_AddBuyerRetentionToUserLeasingAgreement")]
    public class AddBuyerRetentionToUserLeasingAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "buyer_retention_percentage",
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
                name: "buyer_retention_percentage",
                schema: "dbo",
                table: "UserLeasingAgreement");
        }
    }
}
