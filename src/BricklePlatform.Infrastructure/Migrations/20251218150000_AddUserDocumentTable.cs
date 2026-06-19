using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDocumentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[UserDocument] (
                        [id] uniqueidentifier NOT NULL,
                        [user_id] uniqueidentifier NOT NULL,
                        [name] nvarchar(200) NOT NULL,
                        [document_url] nvarchar(500) NOT NULL,
                        [status] nvarchar(50) NOT NULL,
                        [observation] nvarchar(max) NULL,
                        [created_at] datetime2 NOT NULL,
                        [updated_at] datetime2 NOT NULL,
                        CONSTRAINT [PK_UserDocument] PRIMARY KEY ([id]),
                        CONSTRAINT [FK_UserDocument_User_user_id] FOREIGN KEY ([user_id]) REFERENCES [dbo].[User] ([id]) ON DELETE CASCADE
                    );

                    CREATE INDEX [IX_UserDocument_user_id] ON [dbo].[UserDocument] ([user_id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDocument",
                schema: "dbo");
        }
    }
}
