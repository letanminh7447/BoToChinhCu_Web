using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class AddDonMangVeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChiTietDonMangVe",
                columns: table => new
                {
                    Id_ChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Don = table.Column<int>(type: "int", nullable: false),
                    Id_MonAn = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonMangVe", x => x.Id_ChiTiet);
                });

            migrationBuilder.CreateTable(
                name: "DonMangVe",
                columns: table => new
                {
                    Id_Don = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_TK = table.Column<int>(type: "int", nullable: false),
                    HinhThucNhan = table.Column<int>(type: "int", nullable: false),
                    DiaChiGiaoHang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhoangCachKm = table.Column<double>(type: "float", nullable: false),
                    PhiGiaoHang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongTienMon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongThanhToan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonMangVe", x => x.Id_Don);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonMangVe");

            migrationBuilder.DropTable(
                name: "DonMangVe");
        }
    }
}
