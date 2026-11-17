using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HojasPorMes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HojaId",
                table: "Liquidaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Hojas",
                columns: table => new
                {
                    HojaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hojas", x => x.HojaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_HojaId",
                table: "Liquidaciones",
                column: "HojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Hojas_Anio_Mes",
                table: "Hojas",
                columns: new[] { "Anio", "Mes" },
                unique: true);

            // ===== Liquidaciones que ya existían =====
            // Antes de la FK: todas quedaron con HojaId = 0, y 0 no es una hoja.
            // El mes se toma de FechaCreacion en hora de Perú (igual que Hoja.PeriodoDe en el código).

            // 1. Una hoja por cada mes que tiene liquidaciones, con nombre "Noviembre 2026"
            migrationBuilder.Sql(@"
                INSERT INTO ""Hojas"" (""Nombre"", ""Anio"", ""Mes"", ""FechaCreacion"", ""FechaActualizacion"")
                SELECT (ARRAY['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio','Agosto',
                              'Septiembre','Octubre','Noviembre','Diciembre'])[p.mes] || ' ' || p.anio,
                       p.anio, p.mes, NOW(), NOW()
                FROM (
                    SELECT DISTINCT
                        EXTRACT(YEAR  FROM ""FechaCreacion"" AT TIME ZONE 'America/Lima')::int AS anio,
                        EXTRACT(MONTH FROM ""FechaCreacion"" AT TIME ZONE 'America/Lima')::int AS mes
                    FROM ""Liquidaciones""
                ) p;");

            // 2. Cada liquidación a la hoja de su mes
            migrationBuilder.Sql(@"
                UPDATE ""Liquidaciones"" l
                SET ""HojaId"" = h.""HojaId""
                FROM ""Hojas"" h
                WHERE h.""Anio"" = EXTRACT(YEAR  FROM l.""FechaCreacion"" AT TIME ZONE 'America/Lima')::int
                  AND h.""Mes""  = EXTRACT(MONTH FROM l.""FechaCreacion"" AT TIME ZONE 'America/Lima')::int;");

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidaciones_Hojas_HojaId",
                table: "Liquidaciones",
                column: "HojaId",
                principalTable: "Hojas",
                principalColumn: "HojaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liquidaciones_Hojas_HojaId",
                table: "Liquidaciones");

            migrationBuilder.DropTable(
                name: "Hojas");

            migrationBuilder.DropIndex(
                name: "IX_Liquidaciones_HojaId",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "HojaId",
                table: "Liquidaciones");
        }
    }
}
