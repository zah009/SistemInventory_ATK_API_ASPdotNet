using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atk.Migrations
{
    /// <inheritdoc />
    public partial class AddPengadaanLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PengadaanBarangs_Suppliers_SupplierId",
                table: "PengadaanBarangs");

            migrationBuilder.AddColumn<int>(
                name: "BarangId",
                table: "PengadaanBarangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JumlahDiterima",
                table: "PengadaanBarangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PengadaanBarangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PengadaanId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PengadaanId",
                table: "BarangMasuks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PengadaanBarangs_BarangId",
                table: "PengadaanBarangs",
                column: "BarangId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PengadaanId",
                table: "Payments",
                column: "PengadaanId");

            migrationBuilder.CreateIndex(
                name: "IX_BarangMasuks_PengadaanId",
                table: "BarangMasuks",
                column: "PengadaanId");

            migrationBuilder.AddForeignKey(
                name: "FK_BarangMasuks_PengadaanBarangs_PengadaanId",
                table: "BarangMasuks",
                column: "PengadaanId",
                principalTable: "PengadaanBarangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PengadaanBarangs_PengadaanId",
                table: "Payments",
                column: "PengadaanId",
                principalTable: "PengadaanBarangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PengadaanBarangs_Barangs_BarangId",
                table: "PengadaanBarangs",
                column: "BarangId",
                principalTable: "Barangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PengadaanBarangs_Suppliers_SupplierId",
                table: "PengadaanBarangs",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarangMasuks_PengadaanBarangs_PengadaanId",
                table: "BarangMasuks");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PengadaanBarangs_PengadaanId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_PengadaanBarangs_Barangs_BarangId",
                table: "PengadaanBarangs");

            migrationBuilder.DropForeignKey(
                name: "FK_PengadaanBarangs_Suppliers_SupplierId",
                table: "PengadaanBarangs");

            migrationBuilder.DropIndex(
                name: "IX_PengadaanBarangs_BarangId",
                table: "PengadaanBarangs");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PengadaanId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_BarangMasuks_PengadaanId",
                table: "BarangMasuks");

            migrationBuilder.DropColumn(
                name: "BarangId",
                table: "PengadaanBarangs");

            migrationBuilder.DropColumn(
                name: "JumlahDiterima",
                table: "PengadaanBarangs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PengadaanBarangs");

            migrationBuilder.DropColumn(
                name: "PengadaanId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PengadaanId",
                table: "BarangMasuks");

            migrationBuilder.AddForeignKey(
                name: "FK_PengadaanBarangs_Suppliers_SupplierId",
                table: "PengadaanBarangs",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
