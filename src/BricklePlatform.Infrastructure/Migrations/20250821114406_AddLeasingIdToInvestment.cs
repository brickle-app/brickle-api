using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeasingIdToInvestment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "leasing_id",
                schema: "dbo",
                table: "Investment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Investment_leasing_id",
                schema: "dbo",
                table: "Investment",
                column: "leasing_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Investment_Leasing_leasing_id",
                schema: "dbo",
                table: "Investment",
                column: "leasing_id",
                principalSchema: "dbo",
                principalTable: "Leasing",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investment_Leasing_leasing_id",
                schema: "dbo",
                table: "Investment");

            migrationBuilder.DropIndex(
                name: "IX_Investment_leasing_id",
                schema: "dbo",
                table: "Investment");

            migrationBuilder.DropColumn(
                name: "leasing_id",
                schema: "dbo",
                table: "Investment");
        }
    }
}
