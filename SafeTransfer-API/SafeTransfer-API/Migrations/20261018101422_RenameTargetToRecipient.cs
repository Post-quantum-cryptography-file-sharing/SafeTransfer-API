using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTransfer_API.Migrations
{
    /// <inheritdoc />
    public partial class RenameTargetToRecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "UploadBatches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RecipientUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipientUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipientUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecipientUsers_UploadBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "UploadBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipientUsers_BatchId_UserId",
                table: "RecipientUsers",
                columns: new[] { "BatchId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipientUsers_UserId",
                table: "RecipientUsers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipientUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "UploadBatches",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "TargetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TargetUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TargetUsers_UploadBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "UploadBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TargetUsers_BatchId_UserId",
                table: "TargetUsers",
                columns: new[] { "BatchId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetUsers_UserId",
                table: "TargetUsers",
                column: "UserId");
        }
    }
}
