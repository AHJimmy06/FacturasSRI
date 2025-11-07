using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturasSRI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "Clientes",
            //     columns: table => new
            //     {
            //         Id = table.Column<Guid>(type: "uuid", nullable: false),
            //         TipoIdentificacion = table.Column<int>(type: "integer", nullable: false),
            //         NumeroIdentificacion = table.Column<string>(type: "text", nullable: false),
            //         RazonSocial = table.Column<string>(type: "text", nullable: false),
            //         Email = table.Column<string>(type: "text", nullable: false),
            //         Direccion = table.Column<string>(type: "text", nullable: false),
            //         Telefono = table.Column<string>(type: "text", nullable: false),
            //         EstaActivo = table.Column<bool>(type: "boolean", nullable: false),
            //         UsuarioIdCreador = table.Column<Guid>(type: "uuid", nullable: false),
            //         FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Clientes", x => x.Id);
            //     });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AjustesInventario");

            migrationBuilder.DropTable(
                name: "CuentasPorPagar");

            migrationBuilder.DropTable(
                name: "FacturaDetalleConsumoLotes");

            migrationBuilder.DropTable(
                name: "FacturasSRI");

            migrationBuilder.DropTable(
                name: "NotaDeCreditoDetalles");

            migrationBuilder.DropTable(
                name: "NotasDeCreditoSRI");

            migrationBuilder.DropTable(
                name: "PermisoRol");

            migrationBuilder.DropTable(
                name: "PreciosEspeciales");

            migrationBuilder.DropTable(
                name: "ProductoComponentes");

            migrationBuilder.DropTable(
                name: "ProductoImpuestos");

            migrationBuilder.DropTable(
                name: "PuntosEmision");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "FacturaDetalles");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "NotasDeCredito");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Impuestos");

            migrationBuilder.DropTable(
                name: "Establecimientos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
