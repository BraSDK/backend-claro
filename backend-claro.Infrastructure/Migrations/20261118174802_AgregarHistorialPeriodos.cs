using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_claro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialPeriodos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
            name: "HistorialPeriodos",
            table: "Liquidaciones",
            type: "text[]",
            nullable: false,
            defaultValue: Array.Empty<string>());
                
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HistorialPeriodos",
                table: "Liquidaciones");
        }
    }
}
