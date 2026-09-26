using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNationalTeam",
                table: "Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "NationID",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_NationID",
                table: "Teams",
                column: "NationID",
                unique: true,
                filter: "[NationID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Nations_NationID",
                table: "Teams",
                column: "NationID",
                principalTable: "Nations",
                principalColumn: "NationID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Nations_NationID",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_NationID",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsNationalTeam",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "NationID",
                table: "Teams");
        }
    }
}
