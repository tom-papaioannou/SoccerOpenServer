using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerOpenServer.Migrations
{
    /// <inheritdoc />
    public partial class AddHeadingStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Heading",
                table: "PlayerStats",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Heading",
                table: "PlayerStats");
        }
    }
}
