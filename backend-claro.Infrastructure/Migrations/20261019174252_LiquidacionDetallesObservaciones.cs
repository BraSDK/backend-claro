using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LiquidacionDetallesObservaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Ordenes_OrdenTrabajoId",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "AuditadoPorCuentaId",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "Estado",
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
                name: "MotivoNoPago",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ObservacionAuditoria",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ObservacionPago",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "PrecioTotal",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "SinPago",
                table: "Ordenes");

            migrationBuilder.RenameColumn(
                name: "EstadoPago",
                table: "Ordenes",
                newName: "EstadoCampo");

            migrationBuilder.RenameColumn(
                name: "Descripcion",
                table: "Ordenes",
                newName: "DocTecnico");

            migrationBuilder.RenameColumn(
                name: "OrdenTrabajoId",
                table: "Detalles",
                newName: "LiquidacionId");

            migrationBuilder.RenameColumn(
                name: "DetalleTrabajoId",
                table: "Detalles",
                newName: "DetalleLiquidacionId");

            migrationBuilder.RenameIndex(
                name: "IX_Detalles_OrdenTrabajoId",
                table: "Detalles",
                newName: "IX_Detalles_LiquidacionId");

            migrationBuilder.AddColumn<string>(
                name: "TrabajoRealizado",
                table: "Ordenes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioTotal",
                table: "Detalles",
                type: "numeric(18,3)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "Detalles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "CostoAdicionalId",
                table: "Detalles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Liquidaciones",
                columns: table => new
                {
                    LiquidacionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sot = table.Column<long>(type: "bigint", nullable: false),
                    TipoOrden = table.Column<string>(type: "text", nullable: true),
                    NombreSubTipoOrden = table.Column<string>(type: "text", nullable: false),
                    FechaProgramacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NombreTecnico = table.Column<string>(type: "text", nullable: false),
                    EstadoSistema = table.Column<int>(type: "integer", nullable: false),
                    PrecioTotalReal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PrecioTotalEditable = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EstadoSot = table.Column<string>(type: "text", nullable: true),
                    OrdenTrabajoId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    EstadoPago = table.Column<int>(type: "integer", nullable: false),
                    Descuento = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SinPago = table.Column<bool>(type: "boolean", nullable: false),
                    FechaValidacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditadoPorCuentaId = table.Column<int>(type: "integer", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liquidaciones", x => x.LiquidacionId);
                    table.ForeignKey(
                        name: "FK_Liquidaciones_Ordenes_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalTable: "Ordenes",
                        principalColumn: "OrdenTrabajoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Liquidaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "motivos",
                columns: table => new
                {
                    MotivoObservacionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motivos", x => x.MotivoObservacionId);
                });

            migrationBuilder.CreateTable(
                name: "Observaciones",
                columns: table => new
                {
                    ObservacionLiquidacionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LiquidacionId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    MotivoObservacionId = table.Column<int>(type: "integer", nullable: true),
                    Texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observaciones", x => x.ObservacionLiquidacionId);
                    table.ForeignKey(
                        name: "FK_Observaciones_Liquidaciones_LiquidacionId",
                        column: x => x.LiquidacionId,
                        principalTable: "Liquidaciones",
                        principalColumn: "LiquidacionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Observaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observaciones_motivos_MotivoObservacionId",
                        column: x => x.MotivoObservacionId,
                        principalTable: "motivos",
                        principalColumn: "MotivoObservacionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Detalles_CostoAdicionalId",
                table: "Detalles",
                column: "CostoAdicionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_OrdenTrabajoId",
                table: "Liquidaciones",
                column: "OrdenTrabajoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_Sot",
                table: "Liquidaciones",
                column: "Sot",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Liquidaciones_UsuarioId",
                table: "Liquidaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_LiquidacionId",
                table: "Observaciones",
                column: "LiquidacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_MotivoObservacionId",
                table: "Observaciones",
                column: "MotivoObservacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_UsuarioId",
                table: "Observaciones",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Adicionales_CostoAdicionalId",
                table: "Detalles",
                column: "CostoAdicionalId",
                principalTable: "Adicionales",
                principalColumn: "AdicionalId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Liquidaciones_LiquidacionId",
                table: "Detalles",
                column: "LiquidacionId",
                principalTable: "Liquidaciones",
                principalColumn: "LiquidacionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Adicionales_CostoAdicionalId",
                table: "Detalles");

            migrationBuilder.DropForeignKey(
                name: "FK_Detalles_Liquidaciones_LiquidacionId",
                table: "Detalles");

            migrationBuilder.DropTable(
                name: "Observaciones");

            migrationBuilder.DropTable(
                name: "Liquidaciones");

            migrationBuilder.DropTable(
                name: "motivos");

            migrationBuilder.DropIndex(
                name: "IX_Detalles_CostoAdicionalId",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "TrabajoRealizado",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "CostoAdicionalId",
                table: "Detalles");

            migrationBuilder.RenameColumn(
                name: "EstadoCampo",
                table: "Ordenes",
                newName: "EstadoPago");

            migrationBuilder.RenameColumn(
                name: "DocTecnico",
                table: "Ordenes",
                newName: "Descripcion");

            migrationBuilder.RenameColumn(
                name: "LiquidacionId",
                table: "Detalles",
                newName: "OrdenTrabajoId");

            migrationBuilder.RenameColumn(
                name: "DetalleLiquidacionId",
                table: "Detalles",
                newName: "DetalleTrabajoId");

            migrationBuilder.RenameIndex(
                name: "IX_Detalles_LiquidacionId",
                table: "Detalles",
                newName: "IX_Detalles_OrdenTrabajoId");

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

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Ordenes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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
                name: "MotivoNoPago",
                table: "Ordenes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionAuditoria",
                table: "Ordenes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionPago",
                table: "Ordenes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioTotal",
                table: "Ordenes",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SinPago",
                table: "Ordenes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioTotal",
                table: "Detalles",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Concepto",
                table: "Detalles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddForeignKey(
                name: "FK_Detalles_Ordenes_OrdenTrabajoId",
                table: "Detalles",
                column: "OrdenTrabajoId",
                principalTable: "Ordenes",
                principalColumn: "OrdenTrabajoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
