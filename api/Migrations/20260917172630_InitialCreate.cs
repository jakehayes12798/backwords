using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backwords.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lexemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Term = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false),
                    IsSeedWord = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lexemes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Derivations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TermId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedTermId = table.Column<int>(type: "INTEGER", nullable: true),
                    RelationType = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Derivations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Derivations_Lexemes_RelatedTermId",
                        column: x => x.RelatedTermId,
                        principalTable: "Lexemes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Derivations_Lexemes_TermId",
                        column: x => x.TermId,
                        principalTable: "Lexemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Derivations_RelatedTermId",
                table: "Derivations",
                column: "RelatedTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Derivations_TermId",
                table: "Derivations",
                column: "TermId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Derivations");

            migrationBuilder.DropTable(
                name: "Lexemes");
        }
    }
}
