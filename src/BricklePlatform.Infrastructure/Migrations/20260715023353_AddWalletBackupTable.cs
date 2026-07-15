using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletBackupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WalletBackup",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    wallet_address = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    encrypted_private_key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    encryption_version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    cipher = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    kdf = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    kdf_params_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_restored_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBackup", x => x.id);
                    table.ForeignKey(
                        name: "FK_WalletBackup_User_user_id",
                        column: x => x.user_id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletBackup_user_id",
                schema: "dbo",
                table: "WalletBackup",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletBackup_wallet_address",
                schema: "dbo",
                table: "WalletBackup",
                column: "wallet_address",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletBackup",
                schema: "dbo");
        }
    }
}
