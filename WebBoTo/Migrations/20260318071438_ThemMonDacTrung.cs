using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class ThemMonDacTrung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "MonAn",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDacTrung",
                table: "MonAn",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDacTrung",
                table: "MonAn");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "MonAn",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
