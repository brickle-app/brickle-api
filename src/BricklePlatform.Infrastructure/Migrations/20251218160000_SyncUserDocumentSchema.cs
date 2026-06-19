using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    public partial class SyncUserDocumentSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND type in (N'U'))
                BEGIN
                    -- Add missing columns one by one
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'id')
                        ALTER TABLE [dbo].[UserDocument] ADD [id] uniqueidentifier NOT NULL DEFAULT NEWID();

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'user_id')
                        ALTER TABLE [dbo].[UserDocument] ADD [user_id] uniqueidentifier NULL; -- Initial NULL to allow existing rows

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'name')
                        ALTER TABLE [dbo].[UserDocument] ADD [name] nvarchar(200) NULL;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'document_url')
                        ALTER TABLE [dbo].[UserDocument] ADD [document_url] nvarchar(500) NULL;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'status')
                        ALTER TABLE [dbo].[UserDocument] ADD [status] nvarchar(50) NOT NULL DEFAULT 'PENDING';

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'observation')
                        ALTER TABLE [dbo].[UserDocument] ADD [observation] nvarchar(max) NULL;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'created_at')
                        ALTER TABLE [dbo].[UserDocument] ADD [created_at] datetime2 NOT NULL DEFAULT GETDATE();

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'updated_at')
                        ALTER TABLE [dbo].[UserDocument] ADD [updated_at] datetime2 NOT NULL DEFAULT GETDATE();

                    -- Add Index if not exists
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]') AND name = 'IX_UserDocument_user_id')
                        CREATE INDEX [IX_UserDocument_user_id] ON [dbo].[UserDocument] ([user_id]);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No safety down for sync
        }
    }
}
