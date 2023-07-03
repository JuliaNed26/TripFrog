using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripFrog.Migrations
{
    /// <inheritdoc />
    public partial class RealitonshipsThroughFluentApiAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LanguageUser",
                table: "LanguageUser");

            migrationBuilder.DropIndex(
                name: "IX_LanguageUser_UserId",
                table: "LanguageUser");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LanguageUser");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LanguageUser",
                table: "LanguageUser",
                columns: new[] { "UserId", "LanguageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LanguageUser",
                table: "LanguageUser");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LanguageUser",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LanguageUser",
                table: "LanguageUser",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageUser_UserId",
                table: "LanguageUser",
                column: "UserId");
        }
    }
}
