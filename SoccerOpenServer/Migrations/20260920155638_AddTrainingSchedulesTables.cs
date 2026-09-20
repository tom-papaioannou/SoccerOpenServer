using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingSchedulesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_TrainingSchedule_TrainingScheduleID",
                table: "People");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingSchedule",
                table: "TrainingSchedule");

            migrationBuilder.RenameTable(
                name: "TrainingSchedule",
                newName: "TrainingSchedules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingSchedules",
                table: "TrainingSchedules",
                column: "TrainingScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_People_TrainingSchedules_TrainingScheduleID",
                table: "People",
                column: "TrainingScheduleID",
                principalTable: "TrainingSchedules",
                principalColumn: "TrainingScheduleID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_TrainingSchedules_TrainingScheduleID",
                table: "People");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingSchedules",
                table: "TrainingSchedules");

            migrationBuilder.RenameTable(
                name: "TrainingSchedules",
                newName: "TrainingSchedule");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingSchedule",
                table: "TrainingSchedule",
                column: "TrainingScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_People_TrainingSchedule_TrainingScheduleID",
                table: "People",
                column: "TrainingScheduleID",
                principalTable: "TrainingSchedule",
                principalColumn: "TrainingScheduleID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
