using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class ThemTrangThaiTK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TrangThai",
                table: "TaiKhoan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id_TK",
                keyValue: 1,
                column: "TrangThai",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "TaiKhoan");
        }
    }
}
