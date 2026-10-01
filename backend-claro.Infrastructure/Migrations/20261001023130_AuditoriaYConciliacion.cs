using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaYConciliacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuditadoPorCuentaId",
                table: "Ordenes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Descuento",
                table: "Ordenes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaApelacion",
                table: "Ordenes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAuditoria",
                table: "Ordenes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarcadaApelacion",
                table: "Ordenes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionAuditoria",
                table: "Ordenes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SinPago",
                table: "Ordenes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditadoPorCuentaId",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "FechaApelacion",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "FechaAuditoria",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "MarcadaApelacion",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ObservacionAuditoria",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "SinPago",
                table: "Ordenes");
        }
    }
}
