using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffItems_WriteOffReasons_WriteOffReasonID",
                table: "WriteOffItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffReasons_WriteOffItems_WriteOffItemID",
                table: "WriteOffReasons");

            migrationBuilder.DropIndex(
                name: "IX_WriteOffReasons_WriteOffItemID",
                table: "WriteOffReasons");

            migrationBuilder.DropIndex(
                name: "IX_WriteOffItems_WriteOffReasonID",
                table: "WriteOffItems");

            migrationBuilder.DropColumn(
                name: "WriteOffItemID",
                table: "WriteOffReasons");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "Discounts",
                newName: "DiscountPercentage");

            migrationBuilder.AddColumn<int>(
                name: "WriteOff_ReasonID",
                table: "WriteOffItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffItems_WriteOff_ReasonID",
                table: "WriteOffItems",
                column: "WriteOff_ReasonID");

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffItems_WriteOffReasons_WriteOff_ReasonID",
                table: "WriteOffItems",
                column: "WriteOff_ReasonID",
                principalTable: "WriteOffReasons",
                principalColumn: "WriteOff_ReasonID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WriteOffItems_WriteOffReasons_WriteOff_ReasonID",
                table: "WriteOffItems");

            migrationBuilder.DropIndex(
                name: "IX_WriteOffItems_WriteOff_ReasonID",
                table: "WriteOffItems");

            migrationBuilder.DropColumn(
                name: "WriteOff_ReasonID",
                table: "WriteOffItems");

            migrationBuilder.RenameColumn(
                name: "DiscountPercentage",
                table: "Discounts",
                newName: "DiscountAmount");

            migrationBuilder.AddColumn<int>(
                name: "WriteOffItemID",
                table: "WriteOffReasons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffReasons_WriteOffItemID",
                table: "WriteOffReasons",
                column: "WriteOffItemID");

            migrationBuilder.CreateIndex(
                name: "IX_WriteOffItems_WriteOffReasonID",
                table: "WriteOffItems",
                column: "WriteOffReasonID");

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffItems_WriteOffReasons_WriteOffReasonID",
                table: "WriteOffItems",
                column: "WriteOffReasonID",
                principalTable: "WriteOffReasons",
                principalColumn: "WriteOff_ReasonID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WriteOffReasons_WriteOffItems_WriteOffItemID",
                table: "WriteOffReasons",
                column: "WriteOffItemID",
                principalTable: "WriteOffItems",
                principalColumn: "WriteOffItemID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
