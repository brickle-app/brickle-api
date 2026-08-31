using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTypeToUserDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDocument_UserId",
                schema: "dbo",
                table: "UserDocument");

            migrationBuilder.AddColumn<string>(
                name: "document_type",
                schema: "dbo",
                table: "UserDocument",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "IDENTITY");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocument_UserId_DocumentType",
                schema: "dbo",
                table: "UserDocument",
                columns: new[] { "user_id", "document_type" },
                unique: true,
                filter: "[user_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDocument_UserId_DocumentType",
                schema: "dbo",
                table: "UserDocument");

            migrationBuilder.DropColumn(
                name: "document_type",
                schema: "dbo",
                table: "UserDocument");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocument_UserId",
                schema: "dbo",
                table: "UserDocument",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");
        }
    }
}
