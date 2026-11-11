using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NombreDelCambio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Servicios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ServicioCodigo",
                table: "Liquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_ServicioCodigo",
                table: "Liquidaciones",
                column: "ServicioCodigo");

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidaciones_Servicios_ServicioCodigo",
                table: "Liquidaciones",
                column: "ServicioCodigo",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liquidaciones_Servicios_ServicioCodigo",
                table: "Liquidaciones");

            migrationBuilder.DropIndex(
                name: "IX_Liquidaciones_ServicioCodigo",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "ServicioCodigo",
                table: "Liquidaciones");
        }
    }
}
