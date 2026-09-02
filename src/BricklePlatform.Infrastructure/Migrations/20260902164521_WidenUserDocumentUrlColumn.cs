using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WidenUserDocumentUrlColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "document_url",
                schema: "dbo",
                table: "UserDocument",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "document_url",
                schema: "dbo",
                table: "UserDocument",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
