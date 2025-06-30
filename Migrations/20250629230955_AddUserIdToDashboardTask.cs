using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToDashboardTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "DashboardTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DashboardTasks");
        }
    }
}
