using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FacturasSRI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedPermisosRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
              //  name: "PermisoRol");

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("01f6f2c8-2a1e-4c74-8a8f-b9d5c6a1b2d3"), "Acceso a la gestión de clientes", "gestionar-clientes" },
                    { new Guid("b2d0b8c0-3e7a-4f5c-8b1d-9e6a0c7f1a2b"), "Acceso a la creación de facturas", "crear-facturas" },
                    { new Guid("c3e1a9d0-4b8f-4e6c-9a1d-8f7b0c6e2d1a"), "Acceso para ver productos", "ver-productos" },
                    { new Guid("d4f2b8a1-5c9e-4a6b-8c1d-7e6f0a5b3c2d"), "Acceso a la gestión de inventario y lotes", "gestionar-inventario" },
                    { new Guid("e5a3c7b2-6d0f-4b8a-9d2e-6f5b1c4a3e1f"), "Acceso a la gestión de productos", "gestionar-productos" }
                });

            migrationBuilder.InsertData(
                table: "RolPermisos",
                columns: new[] { "PermisosId", "RolesId" },
                values: new object[,]
                {
                    { new Guid("01f6f2c8-2a1e-4c74-8a8f-b9d5c6a1b2d3"), new Guid("e2a87c46-e5b3-4f9e-8c6e-1f2a3b4c5d6e") },
                    { new Guid("b2d0b8c0-3e7a-4f5c-8b1d-9e6a0c7f1a2b"), new Guid("e2a87c46-e5b3-4f9e-8c6e-1f2a3b4c5d6e") },
                    { new Guid("c3e1a9d0-4b8f-4e6c-9a1d-8f7b0c6e2d1a"), new Guid("e2a87c46-e5b3-4f9e-8c6e-1f2a3b4c5d6e") },
                    { new Guid("c3e1a9d0-4b8f-4e6c-9a1d-8f7b0c6e2d1a"), new Guid("f5b8c9d0-1a2b-3c4d-5e6f-7a8b9c0d1e2f") },
                    { new Guid("d4f2b8a1-5c9e-4a6b-8c1d-7e6f0a5b3c2d"), new Guid("f5b8c9d0-1a2b-3c4d-5e6f-7a8b9c0d1e2f") },
                    { new Guid("e5a3c7b2-6d0f-4b8a-9d2e-6f5b1c4a3e1f"), new Guid("f5b8c9d0-1a2b-3c4d-5e6f-7a8b9c0d1e2f") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: new Guid("01f6f2c8-2a1e-4c74-8a8f-b9d5c6a1b2d3"));

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: new Guid("b2d0b8c0-3e7a-4f5c-8b1d-9e6a0c7f1a2b"));

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: new Guid("c3e1a9d0-4b8f-4e6c-9a1d-8f7b0c6e2d1a"));

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: new Guid("d4f2b8a1-5c9e-4a6b-8c1d-7e6f0a5b3c2d"));

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: new Guid("e5a3c7b2-6d0f-4b8a-9d2e-6f5b1c4a3e1f"));

            migrationBuilder.CreateTable(
                name: "PermisoRol",
                columns: table => new
                {
                    PermisosId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisoRol", x => new { x.PermisosId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PermisoRol_Permisos_PermisosId",
                        column: x => x.PermisosId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermisoRol_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermisoRol_RolesId",
                table: "PermisoRol",
                column: "RolesId");
        }
    }
}
