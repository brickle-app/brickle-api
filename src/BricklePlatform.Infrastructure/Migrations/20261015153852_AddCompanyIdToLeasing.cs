using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToLeasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                schema: "dbo",
                table: "Leasing",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leasing_company_id",
                schema: "dbo",
                table: "Leasing",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leasing_Company_company_id",
                schema: "dbo",
                table: "Leasing",
                column: "company_id",
                principalSchema: "dbo",
                principalTable: "Company",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leasing_Company_company_id",
                schema: "dbo",
                table: "Leasing");

            migrationBuilder.DropIndex(
                name: "IX_Leasing_company_id",
                schema: "dbo",
                table: "Leasing");

            migrationBuilder.DropColumn(
                name: "company_id",
                schema: "dbo",
                table: "Leasing");
        }
    }
}
