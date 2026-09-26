using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerPreferredMoves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerPreferredMoves",
                columns: table => new
                {
                    PlayerPreferredMoveID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreferredMove = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPreferredMoves", x => x.PlayerPreferredMoveID);
                    table.ForeignKey(
                        name: "FK_PlayerPreferredMoves_People_PersonID",
                        column: x => x.PersonID,
                        principalTable: "People",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPreferredMoves_PersonID_PreferredMove",
                table: "PlayerPreferredMoves",
                columns: new[] { "PersonID", "PreferredMove" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerPreferredMoves");
        }
    }
}
