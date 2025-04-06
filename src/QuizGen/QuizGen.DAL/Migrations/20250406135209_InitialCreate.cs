using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QuizGen.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    openai_api_key = table.Column<string>(type: "text", nullable: true),
                    gpt_model = table.Column<string>(type: "text", nullable: false, defaultValue: "gpt-4o-mini"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    prompt = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<string>(type: "text", nullable: false),
                    num_questions = table.Column<int>(type: "integer", nullable: false),
                    allowed_types = table.Column<string[]>(type: "text[]", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizzes", x => x.id);
                    table.ForeignKey(
                        name: "FK_quizzes_users_author_id",
                        column: x => x.author_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "questions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    quiz_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_questions_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalSchema: "public",
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_tries",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    quiz_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    score = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_tries", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_tries_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalSchema: "public",
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_tries_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "answers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_answers_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "public",
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    quiz_try_id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    answer_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_answers_answers_answer_id",
                        column: x => x.answer_id,
                        principalSchema: "public",
                        principalTable: "answers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_answers_questions_question_id",
                        column: x => x.question_id,
                        principalSchema: "public",
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_answers_quiz_tries_quiz_try_id",
                        column: x => x.quiz_try_id,
                        principalSchema: "public",
                        principalTable: "quiz_tries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_answers_question_id",
                schema: "public",
                table: "answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_quiz_id",
                schema: "public",
                table: "questions",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_answer_id",
                schema: "public",
                table: "quiz_answers",
                column: "answer_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_question_id",
                schema: "public",
                table: "quiz_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_answers_quiz_try_id",
                schema: "public",
                table: "quiz_answers",
                column: "quiz_try_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_tries_quiz_id",
                schema: "public",
                table: "quiz_tries",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_tries_user_id",
                schema: "public",
                table: "quiz_tries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_author_id",
                schema: "public",
                table: "quizzes",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "public",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_answers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "answers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "quiz_tries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "questions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "quizzes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
