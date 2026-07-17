using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atk.Migrations
{
    /// <inheritdoc />
    public partial class AddDivisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivisiId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Divisis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nama = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisis", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DivisiId",
                table: "Users",
                column: "DivisiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Divisis_DivisiId",
                table: "Users",
                column: "DivisiId",
                principalTable: "Divisis",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Divisis_DivisiId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Divisis");

            migrationBuilder.DropIndex(
                name: "IX_Users_DivisiId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DivisiId",
                table: "Users");
        }
    }
}
