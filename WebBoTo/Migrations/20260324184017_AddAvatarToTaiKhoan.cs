using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarToTaiKhoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id_TK",
                keyValue: 1,
                column: "Avatar",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "TaiKhoan");
        }
    }
}
