using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamInformations",
                columns: table => new
                {
                    TeamID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reputation = table.Column<byte>(type: "tinyint", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransferBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WageBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    YearFounded = table.Column<int>(type: "int", nullable: false),
                    TrainingFacilitiesLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInformations", x => x.TeamID);
                    table.CheckConstraint("CK_TeamInformations_Finances", "[Balance] >= 0 AND [TransferBudget] >= 0 AND [WageBudget] >= 0");
                    table.CheckConstraint("CK_TeamInformations_Reputation", "[Reputation] BETWEEN 1 AND 100");
                    table.CheckConstraint("CK_TeamInformations_TrainingFacilitiesLevel", "[TrainingFacilitiesLevel] BETWEEN 1 AND 6");
                    table.CheckConstraint("CK_TeamInformations_YearFounded", "[YearFounded] BETWEEN 1850 AND 9999");
                    table.ForeignKey(
                        name: "FK_TeamInformations_Teams_TeamID",
                        column: x => x.TeamID,
                        principalTable: "Teams",
                        principalColumn: "TeamID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamInformations");
        }
    }
}
