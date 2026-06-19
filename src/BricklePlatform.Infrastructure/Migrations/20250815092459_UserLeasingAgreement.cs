using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserLeasingAgreement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monthly_canon",
                schema: "dbo",
                table: "Leasing");

            migrationBuilder.DropColumn(
                name: "risk_level",
                schema: "dbo",
                table: "Leasing");

            migrationBuilder.RenameColumn(
                name: "LeasingCoreAddress",
                schema: "dbo",
                table: "UserLeasingAgreement",
                newName: "leasing_address");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_value",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "remaining_balance",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "installment_amount",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "leasing_address",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "agreement_type",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "asset_value",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ibr_rate",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "installment_rate",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "insurance_percentage",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "management_fee",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "risk_level",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "risk_rate",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "term_time",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "useful_life",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "agreement_type",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "asset_value",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "ibr_rate",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "installment_rate",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "insurance_percentage",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "iva",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "management_fee",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "risk_level",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "risk_rate",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "term_time",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.DropColumn(
                name: "useful_life",
                schema: "dbo",
                table: "UserLeasingAgreement");

            migrationBuilder.RenameColumn(
                name: "leasing_address",
                schema: "dbo",
                table: "UserLeasingAgreement",
                newName: "LeasingCoreAddress");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_value",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "remaining_balance",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "installment_amount",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<string>(
                name: "LeasingCoreAddress",
                schema: "dbo",
                table: "UserLeasingAgreement",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_canon",
                schema: "dbo",
                table: "Leasing",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "risk_level",
                schema: "dbo",
                table: "Leasing",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);
        }
    }
}
