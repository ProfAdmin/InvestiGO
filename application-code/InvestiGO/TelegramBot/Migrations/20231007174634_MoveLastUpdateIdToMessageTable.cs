using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBot.Migrations
{
    /// <inheritdoc />
    public partial class MoveLastUpdateIdToMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdateId",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "LastUpdateId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdateId",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "LastUpdateId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
