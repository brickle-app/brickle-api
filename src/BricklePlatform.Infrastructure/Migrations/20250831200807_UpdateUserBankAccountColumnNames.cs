using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserBankAccountColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBankAccount_User_UserId",
                schema: "dbo",
                table: "UserBankAccount");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BankName",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "bank_name");

            migrationBuilder.RenameColumn(
                name: "AccountType",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "account_type");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "account_number");

            migrationBuilder.RenameColumn(
                name: "AccountImage",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "account_image");

            migrationBuilder.RenameColumn(
                name: "AccountHolder",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "account_holder");

            migrationBuilder.RenameColumn(
                name: "AccountDocument",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "account_document");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBankAccount_User_user_id",
                schema: "dbo",
                table: "UserBankAccount",
                column: "user_id",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBankAccount_User_user_id",
                schema: "dbo",
                table: "UserBankAccount");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "bank_name",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "BankName");

            migrationBuilder.RenameColumn(
                name: "account_type",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "AccountType");

            migrationBuilder.RenameColumn(
                name: "account_number",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "AccountNumber");

            migrationBuilder.RenameColumn(
                name: "account_image",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "AccountImage");

            migrationBuilder.RenameColumn(
                name: "account_holder",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "AccountHolder");

            migrationBuilder.RenameColumn(
                name: "account_document",
                schema: "dbo",
                table: "UserBankAccount",
                newName: "AccountDocument");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBankAccount_User_UserId",
                schema: "dbo",
                table: "UserBankAccount",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
