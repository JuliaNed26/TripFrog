using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripFrogWebApi.Migrations
{
    /// <inheritdoc />
    public partial class notMandatoryRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RefreshToken_RefreshTokenId",
                table: "UserRepository");

            migrationBuilder.DropIndex(
                name: "IX_Users_RefreshTokenId",
                table: "UserRepository");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "RefreshToken");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenId",
                table: "UserRepository",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshToken",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RefreshTokenId",
                table: "UserRepository",
                column: "RefreshTokenId",
                unique: true,
                filter: "[RefreshTokenId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_RefreshTokens_RefreshTokenId",
                table: "UserRepository",
                column: "RefreshTokenId",
                principalTable: "RefreshToken",
                principalColumn: "Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RefreshTokens_RefreshTokenId",
                table: "UserRepository");

            migrationBuilder.DropIndex(
                name: "IX_Users_RefreshTokenId",
                table: "UserRepository");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                newName: "RefreshToken");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshTokenId",
                table: "UserRepository",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                table: "RefreshToken",
                column: "Token");

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
    }
}
