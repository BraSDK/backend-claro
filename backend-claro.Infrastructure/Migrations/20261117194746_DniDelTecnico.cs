using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DniDelTecnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocTecnico",
                table: "Liquidaciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Liquidaciones que ya existían: si el nombre coincide con un técnico (Rol 1 = TECNICO),
            // se guarda su DNI y su nombre como está registrado. Solo coincidencia exacta (sin contar mayúsculas
            // ni espacios de los bordes): un nombre incompleto como "Jorge Condori" se queda sin DNI.
            migrationBuilder.Sql(@"
                UPDATE ""Liquidaciones"" l
                SET ""DocTecnico"" = TRIM(u.""DocumentoIdentidad""),
                    ""NombreTecnico"" = TRIM(u.""NombreCompleto"")
                FROM ""Usuarios"" u
                JOIN ""CuentaUsuarios"" c ON c.""Id"" = u.""AuthId""
                WHERE c.""Rol"" = 1
                  AND LOWER(TRIM(u.""NombreCompleto"")) = LOWER(TRIM(l.""NombreTecnico""))
                  AND TRIM(u.""DocumentoIdentidad"") <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocTecnico",
                table: "Liquidaciones");
        }
    }
}
