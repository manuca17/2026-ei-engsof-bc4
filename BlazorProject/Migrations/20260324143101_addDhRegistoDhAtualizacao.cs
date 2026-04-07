using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorProject.Migrations
{
    /// <inheritdoc />
    public partial class addDhRegistoDhAtualizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataHoraAtualizacao",
                table: "utilizador",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataHoraRegisto",
                table: "utilizador",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataHoraAtualizacao",
                table: "utilizador");

            migrationBuilder.DropColumn(
                name: "DataHoraRegisto",
                table: "utilizador");
        }
    }
}
