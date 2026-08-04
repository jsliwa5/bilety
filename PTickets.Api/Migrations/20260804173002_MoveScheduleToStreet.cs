using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTickets.Api.Migrations
{
    /// <inheritdoc />
    public partial class MoveScheduleToStreet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Street");

            migrationBuilder.DropColumn(
                name: "PaidParkingSchedule_EndTime",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "PaidParkingSchedule_PaidDays",
                table: "Zones");

            migrationBuilder.DropColumn(
                name: "PaidParkingSchedule_StartTime",
                table: "Zones");

            migrationBuilder.CreateTable(
                name: "Streets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZoneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    RepresentsWholeZone = table.Column<bool>(type: "INTEGER", nullable: false),
                    PaidParkingSchedule_StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    PaidParkingSchedule_EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    PaidParkingSchedule_PaidDays = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streets_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Streets_ZoneId",
                table: "Streets",
                column: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Streets");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PaidParkingSchedule_EndTime",
                table: "Zones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaidParkingSchedule_PaidDays",
                table: "Zones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PaidParkingSchedule_StartTime",
                table: "Zones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Street",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ZoneId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Street", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Street_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Street_ZoneId",
                table: "Street",
                column: "ZoneId");
        }
    }
}
