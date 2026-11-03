using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PagosClaroYMultas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PagoClaroId",
                table: "Liquidaciones",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PagosClaro",
                columns: table => new
                {
                    PagoClaroId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comprobante = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Observacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RegistradoPorCuentaId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosClaro", x => x.PagoClaroId);
                });

            migrationBuilder.CreateTable(
                name: "MultasClaro",
                columns: table => new
                {
                    MultaClaroId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NombreTecnico = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PagoClaroId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultasClaro", x => x.MultaClaroId);
                    table.ForeignKey(
                        name: "FK_MultasClaro_PagosClaro_PagoClaroId",
                        column: x => x.PagoClaroId,
                        principalTable: "PagosClaro",
                        principalColumn: "PagoClaroId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_PagoClaroId",
                table: "Liquidaciones",
                column: "PagoClaroId");

            migrationBuilder.CreateIndex(
                name: "IX_MultasClaro_PagoClaroId",
                table: "MultasClaro",
                column: "PagoClaroId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosClaro_FechaPago",
                table: "PagosClaro",
                column: "FechaPago");

            migrationBuilder.AddForeignKey(
                name: "FK_Liquidaciones_PagosClaro_PagoClaroId",
                table: "Liquidaciones",
                column: "PagoClaroId",
                principalTable: "PagosClaro",
                principalColumn: "PagoClaroId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liquidaciones_PagosClaro_PagoClaroId",
                table: "Liquidaciones");

            migrationBuilder.DropTable(
                name: "MultasClaro");

            migrationBuilder.DropTable(
                name: "PagosClaro");

            migrationBuilder.DropIndex(
                name: "IX_Liquidaciones_PagoClaroId",
                table: "Liquidaciones");

            migrationBuilder.DropColumn(
                name: "PagoClaroId",
                table: "Liquidaciones");
        }
    }
}
