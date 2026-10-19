using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTransfer_API.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFileExpiresAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_ExpiresAt",
                table: "StoredFiles",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StoredFiles_ExpiresAt",
                table: "StoredFiles");
        }
    }
}
