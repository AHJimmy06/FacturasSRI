using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturasSRI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUserEntityNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Usuarios",
                newName: "PrimerNombre");

            migrationBuilder.AddColumn<string>(
                name: "PrimerApellido",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SegundoApellido",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SegundoNombre",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "PasswordHash", "PrimerApellido", "PrimerNombre", "SegundoApellido", "SegundoNombre" },
                values: new object[] { "$2a$11$Z6n8yL.Dq5YkP/8p.G7hA.6y5w9Q8O5O3e2g7i6c4K9B5H3R2a", "Sistema", "Admin", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimerApellido",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SegundoApellido",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SegundoNombre",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "PrimerNombre",
                table: "Usuarios",
                newName: "Nombre");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "Nombre", "PasswordHash" },
                values: new object[] { "Administrador del Sistema", "$2a$11$KnYr45JSbCoMg4Jtkg0GXegC7SegKYTidLxFYYljNwtLH0l024qLG" });
        }
    }
}
