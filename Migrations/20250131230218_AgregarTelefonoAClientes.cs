using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SODERIA_I.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTelefonoAClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "clientes",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "clientes");
        }
    }
}
