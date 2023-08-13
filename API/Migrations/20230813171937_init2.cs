using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Wines_WineID",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WineName",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WineType",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WineVarietal",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "winePrice",
                table: "Inventories",
                newName: "WinePrice");

            migrationBuilder.AlterColumn<decimal>(
                name: "WinePrice",
                table: "Inventories",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "WineID",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VarietalID",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WineTypeID",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_VarietalID",
                table: "Inventories",
                column: "VarietalID");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_WineTypeID",
                table: "Inventories",
                column: "WineTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Varietals_VarietalID",
                table: "Inventories",
                column: "VarietalID",
                principalTable: "Varietals",
                principalColumn: "VarietalID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Wines_WineID",
                table: "Inventories",
                column: "WineID",
                principalTable: "Wines",
                principalColumn: "WineID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_WineTypes_WineTypeID",
                table: "Inventories",
                column: "WineTypeID",
                principalTable: "WineTypes",
                principalColumn: "WineTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Varietals_VarietalID",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Wines_WineID",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_WineTypes_WineTypeID",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_VarietalID",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_WineTypeID",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "VarietalID",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WineTypeID",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "WinePrice",
                table: "Inventories",
                newName: "winePrice");

            migrationBuilder.AlterColumn<double>(
                name: "winePrice",
                table: "Inventories",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "WineID",
                table: "Inventories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "WineName",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WineType",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WineVarietal",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Wines_WineID",
                table: "Inventories",
                column: "WineID",
                principalTable: "Wines",
                principalColumn: "WineID");
        }
    }
}
