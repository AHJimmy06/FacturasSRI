using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturasSRI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNotasDeCreditoFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NotasDeCredito_UsuarioIdCreador",
                table: "NotasDeCredito",
                column: "UsuarioIdCreador");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_UsuarioIdCreador",
                table: "Facturas",
                column: "UsuarioIdCreador");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Usuarios_UsuarioIdCreador",
                table: "Facturas",
                column: "UsuarioIdCreador",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotasDeCredito_Usuarios_UsuarioIdCreador",
                table: "NotasDeCredito",
                column: "UsuarioIdCreador",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Usuarios_UsuarioIdCreador",
                table: "Facturas");

            migrationBuilder.DropForeignKey(
                name: "FK_NotasDeCredito_Usuarios_UsuarioIdCreador",
                table: "NotasDeCredito");

            migrationBuilder.DropIndex(
                name: "IX_NotasDeCredito_UsuarioIdCreador",
                table: "NotasDeCredito");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_UsuarioIdCreador",
                table: "Facturas");
        }
    }
}
