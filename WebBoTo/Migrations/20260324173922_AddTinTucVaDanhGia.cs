using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class AddTinTucVaDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    Id_DanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_TK = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsNoiBat = table.Column<bool>(type: "bit", nullable: false),
                    AdminReply = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.Id_DanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGia_TaiKhoan_Id_TK",
                        column: x => x.Id_TK,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    Id_ThongBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id_TK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.Id_ThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBao_TaiKhoan_Id_TK",
                        column: x => x.Id_TK,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhDanhGia",
                columns: table => new
                {
                    Id_HinhAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_DanhGia = table.Column<int>(type: "int", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhDanhGia", x => x.Id_HinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnhDanhGia_DanhGia_Id_DanhGia",
                        column: x => x.Id_DanhGia,
                        principalTable: "DanhGia",
                        principalColumn: "Id_DanhGia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_Id_TK",
                table: "DanhGia",
                column: "Id_TK");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhDanhGia_Id_DanhGia",
                table: "HinhAnhDanhGia",
                column: "Id_DanhGia");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_Id_TK",
                table: "ThongBao",
                column: "Id_TK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HinhAnhDanhGia");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "DanhGia");
        }
    }
}
