using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilizadorProfileColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "foto_caminho",
                table: "utilizador",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "foto_nome",
                table: "utilizador",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ficheiro_caminho",
                table: "exame_medico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ficheiro_nome",
                table: "exame_medico",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "foto_caminho",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "foto_nome",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "ficheiro_caminho",
                table: "exame_medico");

            migrationBuilder.DropColumn(
                name: "ficheiro_nome",
                table: "exame_medico");
        }
    }
}
