using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class WineController : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines");

            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines");

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines",
                column: "VarietalID",
                principalTable: "Varietals",
                principalColumn: "VarietalID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines",
                column: "WineTypeID",
                principalTable: "WineTypes",
                principalColumn: "WineTypeID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines");

            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines");

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines",
                column: "VarietalID",
                principalTable: "Varietals",
                principalColumn: "VarietalID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines",
                column: "WineTypeID",
                principalTable: "WineTypes",
                principalColumn: "WineTypeID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
