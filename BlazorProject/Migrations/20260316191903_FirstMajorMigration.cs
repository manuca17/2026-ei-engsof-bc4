using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlazorProject.Migrations
{
    /// <inheritdoc />
    public partial class FirstMajorMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "codigo_postal",
                columns: table => new
                {
                    cod_postal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    localidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_codigo_postal", x => x.cod_postal);
                });

            migrationBuilder.CreateTable(
                name: "fatura",
                columns: table => new
                {
                    id_fatura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valor_pago = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    dh_pagamento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    metodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fatura", x => x.id_fatura);
                });

            migrationBuilder.CreateTable(
                name: "paciente",
                columns: table => new
                {
                    id_paciente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    num_porta = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    rua = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cod_postal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dt_nasc = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_paciente", x => x.id_paciente);
                    table.ForeignKey(
                        name: "fk_pac_codpostal",
                        column: x => x.cod_postal,
                        principalTable: "codigo_postal",
                        principalColumn: "cod_postal");
                });

            migrationBuilder.CreateTable(
                name: "utilizador",
                columns: table => new
                {
                    id_utilizador = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_manager = table.Column<bool>(type: "boolean", nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    num_porta = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    rua = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cod_postal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    num_carteira = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    especialidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_utilizador", x => x.id_utilizador);
                    table.ForeignKey(
                        name: "fk_util_codpostal",
                        column: x => x.cod_postal,
                        principalTable: "codigo_postal",
                        principalColumn: "cod_postal");
                });

            migrationBuilder.CreateTable(
                name: "exame_medico",
                columns: table => new
                {
                    id_exame_medico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dh_exame = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resultado = table.Column<string>(type: "text", nullable: true),
                    id_utilizador = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exame_medico", x => x.id_exame_medico);
                    table.ForeignKey(
                        name: "fk_em_utilizador",
                        column: x => x.id_utilizador,
                        principalTable: "utilizador",
                        principalColumn: "id_utilizador");
                });

            migrationBuilder.CreateTable(
                name: "tipo_consulta",
                columns: table => new
                {
                    id_tipo_consulta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    preco_fixo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    preco_hora = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    id_utilizador = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_consulta", x => x.id_tipo_consulta);
                    table.ForeignKey(
                        name: "fk_tc_utilizador",
                        column: x => x.id_utilizador,
                        principalTable: "utilizador",
                        principalColumn: "id_utilizador");
                });

            migrationBuilder.CreateTable(
                name: "consulta",
                columns: table => new
                {
                    id_consulta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dh_inicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    dh_fim = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    valor_hora = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    id_paciente = table.Column<int>(type: "integer", nullable: true),
                    id_exame_medico = table.Column<int>(type: "integer", nullable: true),
                    id_fatura = table.Column<int>(type: "integer", nullable: true),
                    id_tipo_consulta = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consulta", x => x.id_consulta);
                    table.ForeignKey(
                        name: "fk_con_exame",
                        column: x => x.id_exame_medico,
                        principalTable: "exame_medico",
                        principalColumn: "id_exame_medico");
                    table.ForeignKey(
                        name: "fk_con_fatura",
                        column: x => x.id_fatura,
                        principalTable: "fatura",
                        principalColumn: "id_fatura");
                    table.ForeignKey(
                        name: "fk_con_paciente",
                        column: x => x.id_paciente,
                        principalTable: "paciente",
                        principalColumn: "id_paciente");
                    table.ForeignKey(
                        name: "fk_con_tipo",
                        column: x => x.id_tipo_consulta,
                        principalTable: "tipo_consulta",
                        principalColumn: "id_tipo_consulta");
                });

            migrationBuilder.CreateTable(
                name: "anotacao",
                columns: table => new
                {
                    id_anotacao = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    dh_criacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    dh_ultima_edicao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_utilizador = table.Column<int>(type: "integer", nullable: true),
                    id_consulta = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anotacao", x => x.id_anotacao);
                    table.ForeignKey(
                        name: "fk_anot_consulta",
                        column: x => x.id_consulta,
                        principalTable: "consulta",
                        principalColumn: "id_consulta");
                    table.ForeignKey(
                        name: "fk_anot_utilizador",
                        column: x => x.id_utilizador,
                        principalTable: "utilizador",
                        principalColumn: "id_utilizador");
                });

            migrationBuilder.CreateTable(
                name: "estado",
                columns: table => new
                {
                    id_estado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estado_to = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dh_registo = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "now()"),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    id_consulta = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado", x => x.id_estado);
                    table.ForeignKey(
                        name: "fk_est_consulta",
                        column: x => x.id_consulta,
                        principalTable: "consulta",
                        principalColumn: "id_consulta");
                });

            migrationBuilder.CreateTable(
                name: "utilizador_consulta",
                columns: table => new
                {
                    id_utilizador = table.Column<int>(type: "integer", nullable: false),
                    id_consulta = table.Column<int>(type: "integer", nullable: false),
                    is_criador = table.Column<bool>(type: "boolean", nullable: false),
                    convite_aceite = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_utilizador_consulta", x => new { x.id_utilizador, x.id_consulta });
                    table.ForeignKey(
                        name: "fk_uc_consulta",
                        column: x => x.id_consulta,
                        principalTable: "consulta",
                        principalColumn: "id_consulta");
                    table.ForeignKey(
                        name: "fk_uc_utilizador",
                        column: x => x.id_utilizador,
                        principalTable: "utilizador",
                        principalColumn: "id_utilizador");
                });

            migrationBuilder.CreateIndex(
                name: "idx_anotacao_consulta",
                table: "anotacao",
                column: "id_consulta");

            migrationBuilder.CreateIndex(
                name: "idx_anotacao_utilizador",
                table: "anotacao",
                column: "id_utilizador");

            migrationBuilder.CreateIndex(
                name: "idx_consulta_exame",
                table: "consulta",
                column: "id_exame_medico");

            migrationBuilder.CreateIndex(
                name: "idx_consulta_fatura",
                table: "consulta",
                column: "id_fatura");

            migrationBuilder.CreateIndex(
                name: "idx_consulta_paciente",
                table: "consulta",
                column: "id_paciente");

            migrationBuilder.CreateIndex(
                name: "idx_consulta_tipo",
                table: "consulta",
                column: "id_tipo_consulta");

            migrationBuilder.CreateIndex(
                name: "idx_estado_consulta",
                table: "estado",
                column: "id_consulta");

            migrationBuilder.CreateIndex(
                name: "idx_exame_medico_util",
                table: "exame_medico",
                column: "id_utilizador");

            migrationBuilder.CreateIndex(
                name: "idx_paciente_codpostal",
                table: "paciente",
                column: "cod_postal");

            migrationBuilder.CreateIndex(
                name: "idx_tipo_consulta_util",
                table: "tipo_consulta",
                column: "id_utilizador");

            migrationBuilder.CreateIndex(
                name: "idx_utilizador_codpostal",
                table: "utilizador",
                column: "cod_postal");

            migrationBuilder.CreateIndex(
                name: "utilizador_username_key",
                table: "utilizador",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utilizador_consulta_id_consulta",
                table: "utilizador_consulta",
                column: "id_consulta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anotacao");

            migrationBuilder.DropTable(
                name: "estado");

            migrationBuilder.DropTable(
                name: "utilizador_consulta");

            migrationBuilder.DropTable(
                name: "consulta");

            migrationBuilder.DropTable(
                name: "exame_medico");

            migrationBuilder.DropTable(
                name: "fatura");

            migrationBuilder.DropTable(
                name: "paciente");

            migrationBuilder.DropTable(
                name: "tipo_consulta");

            migrationBuilder.DropTable(
                name: "utilizador");

            migrationBuilder.DropTable(
                name: "codigo_postal");
        }
    }
}
