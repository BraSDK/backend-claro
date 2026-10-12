using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DetalleSinServicioYCostoAdicional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Servicios_ServicioCodigo",
                table: "Detalles");

            migrationBuilder.DropIndex(
                name: "IX_Detalles_ServicioCodigo",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "ServicioCodigo",
                table: "Detalles");

            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                table: "Detalles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "Detalles",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Adicionales",
                columns: table => new
                {
                    AdicionalId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ColumnaExcel = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adicionales", x => x.AdicionalId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Adicionales");

            migrationBuilder.DropColumn(
                name: "Concepto",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "Detalles");

            migrationBuilder.AddColumn<int>(
                name: "ServicioCodigo",
                table: "Detalles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Detalles_ServicioCodigo",
                table: "Detalles",
                column: "ServicioCodigo");

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Servicios_ServicioCodigo",
                table: "Detalles",
                column: "ServicioCodigo",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
