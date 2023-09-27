using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    public partial class init4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HourFrequency",
                table: "TimerFrequency",
                newName: "Frequency");

            migrationBuilder.UpdateData(
                table: "BackupTimers",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastBackup",
                value: new DateTime(2023, 9, 27, 22, 9, 41, 287, DateTimeKind.Local).AddTicks(6167));

            migrationBuilder.UpdateData(
                table: "TimerFrequency",
                keyColumn: "Id",
                keyValue: 1,
                column: "Frequency",
                value: 60);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "TimerFrequency",
                newName: "HourFrequency");

            migrationBuilder.UpdateData(
                table: "BackupTimers",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastBackup",
                value: new DateTime(2023, 9, 27, 14, 49, 59, 412, DateTimeKind.Local).AddTicks(8934));

            migrationBuilder.UpdateData(
                table: "TimerFrequency",
                keyColumn: "Id",
                keyValue: 1,
                column: "HourFrequency",
                value: 1);
        }
    }
}
