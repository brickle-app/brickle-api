using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreationCampaignLeasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "useful_life",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                schema: "dbo",
                table: "Leasing",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "campaign_address",
                schema: "dbo",
                table: "Campaign",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "campaign_tx",
                schema: "dbo",
                table: "Campaign",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active",
                schema: "dbo",
                table: "Leasing");

            migrationBuilder.DropColumn(
                name: "campaign_address",
                schema: "dbo",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "campaign_tx",
                schema: "dbo",
                table: "Campaign");

            migrationBuilder.AlterColumn<decimal>(
                name: "useful_life",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
