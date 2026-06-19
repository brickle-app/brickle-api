using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Keys",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Application = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leasing",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tokens = table.Column<int>(type: "int", nullable: false),
                    tokens_available = table.Column<int>(type: "int", nullable: false),
                    price_per_token = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    monthly_canon = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    type = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    risk_level = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    contract_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    liquidity = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    cover_image_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    miniature_image_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    contract_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leasing", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    profile_picture_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    password_hash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    password_salt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    wallet_address = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    terms_accepted = table.Column<bool>(type: "bit", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    country_of_residence = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    document_type = table.Column<int>(type: "int", nullable: true),
                    document_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    kyc_customer_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    kyc_submission_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    push_notification_token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_basic_profile_complete = table.Column<bool>(type: "bit", nullable: false),
                    is_full_profile_complete = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Campaign",
                schema: "dbo",
                columns: table => new
                {
                    leasing_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    min_capital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    max_capital = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    base_token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    brickle_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    update_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaign", x => x.leasing_id);
                    table.ForeignKey(
                        name: "FK_Campaign_Leasing_leasing_id",
                        column: x => x.leasing_id,
                        principalSchema: "dbo",
                        principalTable: "Leasing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserContact",
                schema: "dbo",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contact_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContact", x => new { x.user_id, x.contact_id });
                    table.ForeignKey(
                        name: "FK_UserContact_User_contact_id",
                        column: x => x.contact_id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserContact_User_user_id",
                        column: x => x.user_id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLeasingAgreement",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    leasing_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_term = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    contract_details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    installment_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tokens_purchased = table.Column<int>(type: "int", nullable: false),
                    total_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    remaining_balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LeasingCoreAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeasingAgreement", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserLeasingAgreement_Leasing_leasing_id",
                        column: x => x.leasing_id,
                        principalSchema: "dbo",
                        principalTable: "Leasing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserLeasingAgreement_User_user_id",
                        column: x => x.user_id,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Keys_key",
                schema: "dbo",
                table: "Keys",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_document_number",
                schema: "dbo",
                table: "User",
                column: "document_number",
                unique: true,
                filter: "[document_number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                schema: "dbo",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserContact_contact_id",
                schema: "dbo",
                table: "UserContact",
                column: "contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeasingAgreement_leasing_id",
                schema: "dbo",
                table: "UserLeasingAgreement",
                column: "leasing_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserLeasingAgreement_user_id",
                schema: "dbo",
                table: "UserLeasingAgreement",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campaign",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Keys",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserContact",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UserLeasingAgreement",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Leasing",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "User",
                schema: "dbo");
        }
    }
}
