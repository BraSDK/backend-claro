using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterOrdenDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Servicios_ServicioId",
                table: "Detalles");

            migrationBuilder.RenameColumn(
                name: "ServicioId",
                table: "Detalles",
                newName: "ServicioCodigo");

            migrationBuilder.RenameIndex(
                name: "IX_Detalles_ServicioId",
                table: "Detalles",
                newName: "IX_Detalles_ServicioCodigo");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Ordenes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Detalles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Servicios_ServicioCodigo",
                table: "Detalles",
                column: "ServicioCodigo",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Servicios_ServicioCodigo",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Detalles");

            migrationBuilder.RenameColumn(
                name: "ServicioCodigo",
                table: "Detalles",
                newName: "ServicioId");

            migrationBuilder.RenameIndex(
                name: "IX_Detalles_ServicioCodigo",
                table: "Detalles",
                newName: "IX_Detalles_ServicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Servicios_ServicioId",
                table: "Detalles",
                column: "ServicioId",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
