using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturasSRI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "EstaActivo", "Nombre", "PasswordHash", "Rol" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "admin@facturassri.com", true, "Administrador del Sistema", "$2a$11$Z6n8yL.Dq5YkP/8p.G7hA.6y5w9Q8O5O3e2g7i6c4K9B5H3R2a", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }
    }
}
