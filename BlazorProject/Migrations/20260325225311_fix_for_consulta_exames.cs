using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorProject.Migrations
{
    /// <inheritdoc />
    public partial class fix_for_consulta_exames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_con_exame",
                table: "consulta");

            migrationBuilder.DropIndex(
                name: "idx_consulta_exame",
                table: "consulta");

            migrationBuilder.DropColumn(
                name: "id_exame_medico",
                table: "consulta");

            migrationBuilder.CreateTable(
                name: "exame_medico_consulta",
                columns: table => new
                {
                    id_exame_medico = table.Column<int>(type: "integer", nullable: false),
                    id_consulta = table.Column<int>(type: "integer", nullable: false),
                    id_utilizador = table.Column<int>(type: "integer", nullable: false),
                    dh_registo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IdUtilizadorNavigationIdUtilizador = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exame_medico_consulta", x => new { x.id_exame_medico, x.id_consulta });
                    table.ForeignKey(
                        name: "FK_exame_medico_consulta_utilizador_IdUtilizadorNavigationIdUt~",
                        column: x => x.IdUtilizadorNavigationIdUtilizador,
                        principalTable: "utilizador",
                        principalColumn: "id_utilizador");
                    table.ForeignKey(
                        name: "fk_emc_consulta",
                        column: x => x.id_consulta,
                        principalTable: "consulta",
                        principalColumn: "id_consulta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_emc_exame",
                        column: x => x.id_exame_medico,
                        principalTable: "exame_medico",
                        principalColumn: "id_exame_medico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exame_medico_consulta_id_consulta",
                table: "exame_medico_consulta",
                column: "id_consulta");

            migrationBuilder.CreateIndex(
                name: "IX_exame_medico_consulta_IdUtilizadorNavigationIdUtilizador",
                table: "exame_medico_consulta",
                column: "IdUtilizadorNavigationIdUtilizador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exame_medico_consulta");

            migrationBuilder.AddColumn<int>(
                name: "id_exame_medico",
                table: "consulta",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_consulta_exame",
                table: "consulta",
                column: "id_exame_medico");

            migrationBuilder.AddForeignKey(
                name: "fk_con_exame",
                table: "consulta",
                column: "id_exame_medico",
                principalTable: "exame_medico",
                principalColumn: "id_exame_medico");
        }
    }
}
