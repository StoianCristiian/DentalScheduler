using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentalScheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCostToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Appointments",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Appointments");
        }
    }
}
