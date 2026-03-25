using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorProject.Migrations
{
    /// <inheritdoc />
    public partial class fix_for_pacientes_having_a_creator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdUtilizador",
                table: "paciente",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_paciente_IdUtilizador",
                table: "paciente",
                column: "IdUtilizador");

            migrationBuilder.AddForeignKey(
                name: "fk_pac_utilizador",
                table: "paciente",
                column: "IdUtilizador",
                principalTable: "utilizador",
                principalColumn: "id_utilizador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pac_utilizador",
                table: "paciente");

            migrationBuilder.DropIndex(
                name: "IX_paciente_IdUtilizador",
                table: "paciente");

            migrationBuilder.DropColumn(
                name: "IdUtilizador",
                table: "paciente");
        }
    }
}
