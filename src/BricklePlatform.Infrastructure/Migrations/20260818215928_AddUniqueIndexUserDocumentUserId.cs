using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BricklePlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexUserDocumentUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove pre-existing duplicate rows per user_id before enforcing uniqueness.
            // Keeps, per user: an APPROVED row if one exists, otherwise the most recently created row.
            migrationBuilder.Sql(@"
                ;WITH RankedDocuments AS (
                    SELECT
                        id,
                        ROW_NUMBER() OVER (
                            PARTITION BY user_id
                            ORDER BY CASE WHEN status = 'APPROVED' THEN 0 ELSE 1 END, created_at DESC
                        ) AS rn
                    FROM [dbo].[UserDocument]
                    WHERE user_id IS NOT NULL
                )
                DELETE FROM [dbo].[UserDocument]
                WHERE id IN (SELECT id FROM RankedDocuments WHERE rn > 1);
            ");

            // Drop whichever pre-existing (non-unique) index on user_id is actually present -
            // earlier migrations created it as either IX_UserDocument_UserId or IX_UserDocument_user_id.
            migrationBuilder.Sql(@"
                DECLARE @existingIndex sysname = (
                    SELECT TOP 1 name FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'[dbo].[UserDocument]')
                      AND name IN (N'IX_UserDocument_UserId', N'IX_UserDocument_user_id')
                );
                IF @existingIndex IS NOT NULL
                BEGIN
                    DECLARE @sql nvarchar(max) = N'DROP INDEX [' + @existingIndex + N'] ON [dbo].[UserDocument];';
                    EXEC sp_executesql @sql;
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocument_UserId",
                schema: "dbo",
                table: "UserDocument",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDocument_UserId",
                schema: "dbo",
                table: "UserDocument");

            migrationBuilder.CreateIndex(
                name: "IX_UserDocument_UserId",
                schema: "dbo",
                table: "UserDocument",
                column: "user_id");
        }
    }
}
