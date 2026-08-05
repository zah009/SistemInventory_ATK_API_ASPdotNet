using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atk.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserDivisiRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarangMasuks_Barangs_BarangId",
                table: "BarangMasuks");

            migrationBuilder.AddForeignKey(
                name: "FK_BarangMasuks_Barangs_BarangId",
                table: "BarangMasuks",
                column: "BarangId",
                principalTable: "Barangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BarangMasuks_Barangs_BarangId",
                table: "BarangMasuks");

            migrationBuilder.AddForeignKey(
                name: "FK_BarangMasuks_Barangs_BarangId",
                table: "BarangMasuks",
                column: "BarangId",
                principalTable: "Barangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
