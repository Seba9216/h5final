using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSocketServer.Server.Migrations
{
    /// <inheritdoc />
    public partial class Fixedplayersingamestable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Games_DuckingGameId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DuckingGameId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DuckingGameId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "GamePlayers",
                columns: table => new
                {
                    DuckingGameId = table.Column<int>(type: "int", nullable: false),
                    PlayersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlayers", x => new { x.DuckingGameId, x.PlayersId });
                    table.ForeignKey(
                        name: "FK_GamePlayers_Games_DuckingGameId",
                        column: x => x.DuckingGameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePlayers_Users_PlayersId",
                        column: x => x.PlayersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_PlayersId",
                table: "GamePlayers",
                column: "PlayersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamePlayers");

            migrationBuilder.AddColumn<int>(
                name: "DuckingGameId",
                table: "Users",
                type: "int",
                nullable: true);

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
    }
}
