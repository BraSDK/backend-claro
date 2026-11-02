using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LiquidacionMontoPagado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MontoPagado",
                table: "Liquidaciones",
                type: "numeric(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoPagado",
                table: "Liquidaciones");
        }
    }
}
