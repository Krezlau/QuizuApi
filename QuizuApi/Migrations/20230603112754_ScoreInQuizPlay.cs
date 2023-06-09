using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizuApi.Migrations
{
    /// <inheritdoc />
    public partial class ScoreInQuizPlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "QuizPlays",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuizPlays");
        }
    }
}
