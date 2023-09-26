using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripFrogWebApi.Migrations
{
    /// <inheritdoc />
    public partial class refreshTokenAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenId",
                table: "UserRepository",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Token);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshTokenId",
                table: "UserRepository",
                column: "RefreshTokenId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_RefreshToken_RefreshTokenId",
                table: "UserRepository",
                column: "RefreshTokenId",
                principalTable: "RefreshToken",
                principalColumn: "Token",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RefreshToken_RefreshTokenId",
                table: "UserRepository");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropIndex(
                name: "IX_Users_RefreshTokenId",
                table: "UserRepository");

            migrationBuilder.DropColumn(
                name: "RefreshTokenId",
                table: "UserRepository");
        }
    }
}
