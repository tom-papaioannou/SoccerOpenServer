using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainingScheduleID",
                table: "People",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainingSchedule",
                columns: table => new
                {
                    TrainingScheduleID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingScheduleLevel = table.Column<int>(type: "int", nullable: false),
                    ScheduleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttackPoints = table.Column<byte>(type: "tinyint", nullable: false),
                    DefendPoints = table.Column<byte>(type: "tinyint", nullable: false),
                    ControlPoints = table.Column<byte>(type: "tinyint", nullable: false),
                    GoalkeeperPoints = table.Column<byte>(type: "tinyint", nullable: false),
                    TacticPoints = table.Column<byte>(type: "tinyint", nullable: false),
                    FitnessPoints = table.Column<byte>(type: "tinyint", nullable: false),
                    CoachID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSchedule", x => x.TrainingScheduleID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_People_TrainingScheduleID",
                table: "People",
                column: "TrainingScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_People_TrainingSchedule_TrainingScheduleID",
                table: "People",
                column: "TrainingScheduleID",
                principalTable: "TrainingSchedule",
                principalColumn: "TrainingScheduleID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_TrainingSchedule_TrainingScheduleID",
                table: "People");

            migrationBuilder.DropTable(
                name: "TrainingSchedule");

            migrationBuilder.DropIndex(
                name: "IX_People_TrainingScheduleID",
                table: "People");

            migrationBuilder.DropColumn(
                name: "TrainingScheduleID",
                table: "People");
        }
    }
}
