using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Servicios_y_detalles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubTipo",
                table: "Servicios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CodigoServicio",
                table: "Detalles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Detalles_CodigoServicio",
                table: "Detalles",
                column: "CodigoServicio");

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Servicios_CodigoServicio",
                table: "Detalles",
                column: "CodigoServicio",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Servicios_CodigoServicio",
                table: "Detalles");

            migrationBuilder.DropIndex(
                name: "IX_Detalles_CodigoServicio",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "SubTipo",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "CodigoServicio",
                table: "Detalles");
        }
    }
}
