using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class ThemNhanVienKS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id_NhanVienDuyet",
                table: "DatBan",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenNhanVienDuyet",
                table: "DatBan",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id_NhanVienDuyet",
                table: "DatBan");

            migrationBuilder.DropColumn(
                name: "TenNhanVienDuyet",
                table: "DatBan");
        }
    }
}
