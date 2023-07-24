using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class newpayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Bookings_BookingId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ExpiredDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "TicketPrice",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "Tickets",
                newName: "BookingID");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_BookingId",
                table: "Tickets",
                newName: "IX_Tickets_BookingID");

            migrationBuilder.AlterColumn<int>(
                name: "BookingID",
                table: "Tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventID",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentID",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventPrice",
                table: "Events",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "EarlyBirdID",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Percentage",
                table: "EarlyBird",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateTable(
                name: "EventsPayments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    merchant_id = table.Column<int>(type: "int", nullable: false),
                    merchant_key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<int>(type: "int", nullable: false),
                    item_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cell_number = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventsPayments", x => x.PaymentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EarlyBirdID",
                table: "Events",
                column: "EarlyBirdID");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EarlyBird_EarlyBirdID",
                table: "Events",
                column: "EarlyBirdID",
                principalTable: "EarlyBird",
                principalColumn: "EarlyBirdID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Bookings_BookingID",
                table: "Tickets",
                column: "BookingID",
                principalTable: "Bookings",
                principalColumn: "BookingID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_EarlyBird_EarlyBirdID",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Bookings_BookingID",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "EventsPayments");

            migrationBuilder.DropIndex(
                name: "IX_Events_EarlyBirdID",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EventID",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EarlyBirdID",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "BookingID",
                table: "Tickets",
                newName: "BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_Tickets_BookingID",
                table: "Tickets",
                newName: "IX_Tickets_BookingId");

            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredDate",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchaseDate",
                table: "Tickets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "Tickets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TicketPrice",
                table: "Tickets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "EventPrice",
                table: "Events",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Percentage",
                table: "EarlyBird",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Bookings_BookingId",
                table: "Tickets",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
