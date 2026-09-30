using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PTickets.Modules.Tickets.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalTicketFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tickets");

            migrationBuilder.CreateTable(
                name: "ResidentCards",
                schema: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StreetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StreetZoneMappings",
                schema: "tickets",
                columns: table => new
                {
                    StreetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ZoneId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreetZoneMappings", x => x.StreetId);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExternalTicketId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StreetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParkingZone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResidentCards_RegistrationNumber_StreetId_ValidTo",
                schema: "tickets",
                table: "ResidentCards",
                columns: new[] { "RegistrationNumber", "StreetId", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ExternalTicketId",
                schema: "tickets",
                table: "Tickets",
                column: "ExternalTicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RegistrationNumber_StreetId_ValidTo",
                schema: "tickets",
                table: "Tickets",
                columns: new[] { "RegistrationNumber", "StreetId", "ValidTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResidentCards",
                schema: "tickets");

            migrationBuilder.DropTable(
                name: "StreetZoneMappings",
                schema: "tickets");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "tickets");
        }
    }
}
