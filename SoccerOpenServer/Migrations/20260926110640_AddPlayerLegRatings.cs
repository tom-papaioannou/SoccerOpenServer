using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerLegRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "LeftLegRating",
                table: "PlayerStats",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)25);

            migrationBuilder.AddColumn<byte>(
                name: "RightLegRating",
                table: "PlayerStats",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)85);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PlayerStats_LegRatings",
                table: "PlayerStats",
                sql: "(([RightLegRating] >= 85 AND [LeftLegRating] >= 25 AND [LeftLegRating] < [RightLegRating]) OR ([LeftLegRating] >= 85 AND [RightLegRating] >= 25 AND [RightLegRating] < [LeftLegRating]))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PlayerStats_LegRatings",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "LeftLegRating",
                table: "PlayerStats");

            migrationBuilder.DropColumn(
                name: "RightLegRating",
                table: "PlayerStats");
        }
    }
}
