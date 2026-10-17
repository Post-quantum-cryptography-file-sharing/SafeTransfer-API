using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTransfer_API.Migrations
{
    /// <inheritdoc />
    public partial class FixTargetUserBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TargetUsers_UploadBatches_UploadBatchId",
                table: "TargetUsers");

            migrationBuilder.DropIndex(
                name: "IX_TargetUsers_UploadBatchId",
                table: "TargetUsers");

            migrationBuilder.DropColumn(
                name: "UploadBatchId",
                table: "TargetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UploadBatchId",
                table: "TargetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TargetUsers_UploadBatchId",
                table: "TargetUsers",
                column: "UploadBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_TargetUsers_UploadBatches_UploadBatchId",
                table: "TargetUsers",
                column: "UploadBatchId",
                principalTable: "UploadBatches",
                principalColumn: "Id");
        }
    }
}
