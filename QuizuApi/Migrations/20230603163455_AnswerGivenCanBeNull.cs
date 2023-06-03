using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizuApi.Migrations
{
    /// <inheritdoc />
    public partial class AnswerGivenCanBeNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Answers_AnswerGivenId",
                table: "UserAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "AnswerGivenId",
                table: "UserAnswers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Answers_AnswerGivenId",
                table: "UserAnswers",
                column: "AnswerGivenId",
                principalTable: "Answers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Answers_AnswerGivenId",
                table: "UserAnswers");

            migrationBuilder.AlterColumn<Guid>(
                name: "AnswerGivenId",
                table: "UserAnswers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Answers_AnswerGivenId",
                table: "UserAnswers",
                column: "AnswerGivenId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
