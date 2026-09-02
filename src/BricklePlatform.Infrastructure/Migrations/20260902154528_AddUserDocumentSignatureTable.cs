using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDocumentSignatureTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDocumentSignature",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    document_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    document_version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    signature_data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    signer_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ip_address = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    signed_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDocumentSignature", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserDocumentSignature_User_user_id",
                        column: x => x.user_id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDocumentSignature_UserId_DocumentType",
                schema: "dbo",
                table: "UserDocumentSignature",
                columns: new[] { "user_id", "document_type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDocumentSignature",
                schema: "dbo");
        }
    }
}
