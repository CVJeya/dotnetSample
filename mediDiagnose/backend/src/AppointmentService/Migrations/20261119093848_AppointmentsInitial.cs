using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentsInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppointmentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Shift = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftCapacities",
                columns: table => new
                {
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Shift = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ReservedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftCapacities", x => new { x.DoctorId, x.LocalDate, x.Shift });
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_DoctorId_LocalDate_Shift_Status",
                table: "AppointmentRequests",
                columns: new[] { "DoctorId", "LocalDate", "Shift", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_PatientId_StartsAtUtc",
                table: "AppointmentRequests",
                columns: new[] { "PatientId", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_AppointmentRequests_ActiveSlot",
                table: "AppointmentRequests",
                columns: new[] { "DoctorId", "StartsAtUtc" },
                unique: true,
                filter: "\"Status\" IN ('Pending', 'Approved')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentRequests");

            migrationBuilder.DropTable(
                name: "ShiftCapacities");
        }
    }
}
