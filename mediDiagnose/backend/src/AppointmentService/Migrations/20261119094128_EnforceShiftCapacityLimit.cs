using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.Migrations
{
    /// <inheritdoc />
    public partial class EnforceShiftCapacityLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_ShiftCapacities_ReservedCount",
                table: "ShiftCapacities",
                sql: "\"ReservedCount\" >= 0 AND \"ReservedCount\" <= 20");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ShiftCapacities_ReservedCount",
                table: "ShiftCapacities");
        }
    }
}
