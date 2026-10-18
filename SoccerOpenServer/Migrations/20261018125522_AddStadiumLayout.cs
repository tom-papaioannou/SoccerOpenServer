using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddStadiumLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StadiumLayout",
                table: "Stadium",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StadiumLayout",
                table: "Stadium");
        }
    }
}
