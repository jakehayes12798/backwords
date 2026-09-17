using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backwords.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameLanguageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LanguageCode",
                table: "Lexemes",
                newName: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Lexemes",
                newName: "LanguageCode");
        }
    }
}
