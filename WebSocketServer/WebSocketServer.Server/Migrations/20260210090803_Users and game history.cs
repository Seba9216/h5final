using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace WebSocketServer.Server.Migrations
{
    /// <inheritdoc />
    public partial class Usersandgamehistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuckingGameId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DuckingGameId",
                table: "Users",
                column: "DuckingGameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Games_DuckingGameId",
                table: "Users",
                column: "DuckingGameId",
                principalTable: "Games",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Games_DuckingGameId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Users_DuckingGameId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DuckingGameId",
                table: "Users");
        }
    }
}
