using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.API.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TaskTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TaskDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                TaskDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                TaskStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                TaskRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                LastUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "System"),
                LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "System")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_TaskTitle",
            table: "Tasks",
            column: "TaskTitle");

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_TaskStatus",
            table: "Tasks",
            column: "TaskStatus");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Tasks_TaskStatus",
            table: "Tasks");

        migrationBuilder.DropIndex(
            name: "IX_Tasks_TaskTitle",
            table: "Tasks");

        migrationBuilder.DropTable(
            name: "Tasks");
    }
}
