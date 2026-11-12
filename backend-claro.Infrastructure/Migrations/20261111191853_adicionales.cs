using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adicionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Codigo",
                table: "Adicionales",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServicioCodigo",
                table: "Adicionales",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adicionales_ServicioCodigo",
                table: "Adicionales",
                column: "ServicioCodigo");

            migrationBuilder.AddForeignKey(
                name: "FK_Adicionales_Servicios_ServicioCodigo",
                table: "Adicionales",
                column: "ServicioCodigo",
                principalTable: "Servicios",
                principalColumn: "Codigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adicionales_Servicios_ServicioCodigo",
                table: "Adicionales");

            migrationBuilder.DropIndex(
                name: "IX_Adicionales_ServicioCodigo",
                table: "Adicionales");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Adicionales");

            migrationBuilder.DropColumn(
                name: "ServicioCodigo",
                table: "Adicionales");
        }
    }
}
