using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBoTo.Migrations
{
    /// <inheritdoc />
    public partial class AddDonMangVeTable_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DonMangVe_Id_TK",
                table: "DonMangVe",
                column: "Id_TK");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonMangVe_Id_Don",
                table: "ChiTietDonMangVe",
                column: "Id_Don");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonMangVe_Id_MonAn",
                table: "ChiTietDonMangVe",
                column: "Id_MonAn");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonMangVe_DonMangVe_Id_Don",
                table: "ChiTietDonMangVe",
                column: "Id_Don",
                principalTable: "DonMangVe",
                principalColumn: "Id_Don",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonMangVe_MonAn_Id_MonAn",
                table: "ChiTietDonMangVe",
                column: "Id_MonAn",
                principalTable: "MonAn",
                principalColumn: "Id_MonAn",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonMangVe_TaiKhoan_Id_TK",
                table: "DonMangVe",
                column: "Id_TK",
                principalTable: "TaiKhoan",
                principalColumn: "Id_TK",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonMangVe_DonMangVe_Id_Don",
                table: "ChiTietDonMangVe");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonMangVe_MonAn_Id_MonAn",
                table: "ChiTietDonMangVe");

            migrationBuilder.DropForeignKey(
                name: "FK_DonMangVe_TaiKhoan_Id_TK",
                table: "DonMangVe");

            migrationBuilder.DropIndex(
                name: "IX_DonMangVe_Id_TK",
                table: "DonMangVe");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietDonMangVe_Id_Don",
                table: "ChiTietDonMangVe");

            migrationBuilder.DropIndex(
                name: "IX_ChiTietDonMangVe_Id_MonAn",
                table: "ChiTietDonMangVe");
        }
    }
}
