using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DetallesConCodigoTarifario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Servicios_CodigoServicio",
                table: "Detalles");

            migrationBuilder.AddColumn<int>(
                name: "CodigoTarifario",
                table: "Detalles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Servicios_CodigoServicio",
                table: "Detalles",
                column: "CodigoServicio",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Restrict);

            // ===== Datos que ya existían (solo se enlazan códigos; los precios guardados no se tocan) =====

            // 1. Adicionales guardados sin enlace al catálogo: se enlazan por el nombre (DECO, DROP 150...)
            migrationBuilder.Sql(@"
                UPDATE ""Detalles"" d
                SET ""CostoAdicionalId"" = a.""AdicionalId""
                FROM ""Adicionales"" a
                WHERE d.""CostoAdicionalId"" IS NULL
                  AND d.""Tipo"" = 1
                  AND UPPER(TRIM(a.""Nombre"")) = UPPER(TRIM(d.""Concepto""));");

            // 2. Código del tarifario de cada adicional
            migrationBuilder.Sql(@"
                UPDATE ""Detalles"" d
                SET ""CodigoTarifario"" = a.""Codigo""
                FROM ""Adicionales"" a
                WHERE d.""CostoAdicionalId"" = a.""AdicionalId"";");

            // 3. Costo de servicio de las liquidaciones que ya tienen código de servicio
            migrationBuilder.Sql(@"
                UPDATE ""Detalles"" d
                SET ""CodigoServicio"" = l.""ServicioCodigo"",
                    ""CodigoTarifario"" = l.""ServicioCodigo""
                FROM ""Liquidaciones"" l
                WHERE d.""LiquidacionId"" = l.""LiquidacionId""
                  AND d.""Tipo"" = 0
                  AND UPPER(TRIM(d.""Concepto"")) = 'COSTO DE SERVICIO'
                  AND l.""ServicioCodigo"" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Servicios_CodigoServicio",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "CodigoTarifario",
                table: "Detalles");

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Servicios_CodigoServicio",
                table: "Detalles",
                column: "CodigoServicio",
                principalTable: "Servicios",
                principalColumn: "Codigo",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
