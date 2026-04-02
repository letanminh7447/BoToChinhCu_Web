using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class DatBan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChiTietDatBan",
                columns: table => new
                {
                    Id_CTDB = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_DatBan = table.Column<int>(type: "int", nullable: false),
                    Id_MonAn = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDatBan", x => x.Id_CTDB);
                });

            migrationBuilder.CreateTable(
                name: "DatBan",
                columns: table => new
                {
                    Id_DatBan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_TK = table.Column<int>(type: "int", nullable: false),
                    SoLuongNguoi = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioDat = table.Column<TimeSpan>(type: "time", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatBan", x => x.Id_DatBan);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDatBan");

            migrationBuilder.DropTable(
                name: "DatBan");
        }
    }
}
