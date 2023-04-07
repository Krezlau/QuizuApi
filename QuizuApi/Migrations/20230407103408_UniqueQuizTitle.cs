using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizuApi.Migrations
{
    /// <inheritdoc />
    public partial class UniqueQuizTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_Title",
                table: "Quizzes",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quizzes_Title",
                table: "Quizzes");
        }
    }
}
