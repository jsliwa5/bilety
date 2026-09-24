using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTickets.Modules.Inspections.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ZoneAndStreetOnlyInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inspections");

            migrationBuilder.CreateTable(
                name: "Inspections",
                schema: "inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ZoneId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StreetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    TicketIsValid = table.Column<bool>(type: "INTEGER", nullable: true),
                    TicketValidFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TicketValidTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TicketProviderMessage = table.Column<string>(type: "TEXT", nullable: true),
                    SecondCheckIsValid = table.Column<bool>(type: "INTEGER", nullable: true),
                    SecondCheckValidFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SecondCheckValidTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SecondCheckProviderMessage = table.Column<string>(type: "TEXT", nullable: true),
                    PhotoIds = table.Column<string>(type: "TEXT", nullable: false),
                    NoticeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViolationEntries",
                schema: "inspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ViolationTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViolationEntries_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalSchema: "inspections",
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ViolationEntries_InspectionId",
                schema: "inspections",
                table: "ViolationEntries",
                column: "InspectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "inspections");

            migrationBuilder.DropTable(
                name: "ViolationEntries",
                schema: "inspections");

            migrationBuilder.DropTable(
                name: "Inspections",
                schema: "inspections");
        }
    }
}
