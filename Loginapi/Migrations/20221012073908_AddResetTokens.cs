using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Loginapi.Migrations
{
    public partial class AddResetTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResetToken",
                columns: table => new
                {
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetToken", x => x.Token);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResetToken_Token",
                table: "ResetToken",
                column: "Token",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResetToken");
        }
    }
}
