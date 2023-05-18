using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class NewWine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines");

            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines");

            migrationBuilder.DropColumn(
                name: "WineType",
                table: "Wines");

            migrationBuilder.DropColumn(
                name: "WineVarietal",
                table: "Wines");

            migrationBuilder.AlterColumn<int>(
                name: "WineTypeID",
                table: "Wines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VarietalID",
                table: "Wines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines");

            migrationBuilder.DropForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines");

            migrationBuilder.AlterColumn<int>(
                name: "WineTypeID",
                table: "Wines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "VarietalID",
                table: "Wines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "WineType",
                table: "Wines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WineVarietal",
                table: "Wines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_Varietals_VarietalID",
                table: "Wines",
                column: "VarietalID",
                principalTable: "Varietals",
                principalColumn: "VarietalID");

            migrationBuilder.AddForeignKey(
                name: "FK_Wines_WineTypes_WineTypeID",
                table: "Wines",
                column: "WineTypeID",
                principalTable: "WineTypes",
                principalColumn: "WineTypeID");
        }
    }
}
