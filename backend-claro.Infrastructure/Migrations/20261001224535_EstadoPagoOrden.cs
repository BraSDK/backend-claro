using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EstadoPagoOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoPago",
                table: "Ordenes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MotivoNoPago",
                table: "Ordenes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionPago",
                table: "Ordenes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_Sot",
                table: "Ordenes",
                column: "Sot");

            // SOT ya auditadas antes de este cambio: multa = No pago (2), el resto Sí pago (1); sin auditar queda Pendiente (0)
            migrationBuilder.Sql(@"
                UPDATE ""Ordenes""
                SET ""EstadoPago"" = CASE WHEN ""SinPago"" THEN 2 ELSE 1 END,
                    ""MotivoNoPago"" = CASE WHEN ""SinPago"" THEN 'Otro' ELSE NULL END
                WHERE ""FechaAuditoria"" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ordenes_Sot",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "EstadoPago",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "MotivoNoPago",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ObservacionPago",
                table: "Ordenes");
        }
    }
}
