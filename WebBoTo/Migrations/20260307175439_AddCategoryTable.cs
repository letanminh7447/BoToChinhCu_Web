using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DanhMuc",
                table: "MonAn");

            migrationBuilder.AddColumn<int>(
                name: "DanhMucId",
                table: "MonAn",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    Id_DanhMuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.Id_DanhMuc);
                });

            migrationBuilder.InsertData(
                table: "DanhMuc",
                columns: new[] { "Id_DanhMuc", "TenDanhMuc" },
                values: new object[,]
                {
                    { 1, "Luộc" },
                    { 2, "Hấp" },
                    { 3, "Nướng" },
                    { 4, "Nhúng" },
                    { 5, "Lẩu" },
                    { 6, "Cháo" },
                    { 7, "Gà" },
                    { 8, "Tôm" },
                    { 9, "Mực" },
                    { 10, "Chiên" },
                    { 11, "Xào" },
                    { 12, "Nước Ngọt" },
                    { 13, "Bia - Rượu" },
                    { 14, "Tráng miệng" }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoan",
                columns: new[] { "Id_TK", "Email", "HoTen", "MatKhau", "SoDienThoai", "VaiTro" },
                values: new object[] { 1, "admin@webboto.vn", "Admin", "Admin@123", "0123456789", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_MonAn_DanhMucId",
                table: "MonAn",
                column: "DanhMucId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonAn_DanhMuc_DanhMucId",
                table: "MonAn",
                column: "DanhMucId",
                principalTable: "DanhMuc",
                principalColumn: "Id_DanhMuc",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonAn_DanhMuc_DanhMucId",
                table: "MonAn");

            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropIndex(
                name: "IX_MonAn_DanhMucId",
                table: "MonAn");

            migrationBuilder.DeleteData(
                table: "TaiKhoan",
                keyColumn: "Id_TK",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "DanhMucId",
                table: "MonAn");

            migrationBuilder.AddColumn<string>(
                name: "DanhMuc",
                table: "MonAn",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
