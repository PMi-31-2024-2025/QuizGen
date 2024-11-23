using QuizGen.BLL.Models.Base;
using QuizGen.BLL.Services.Interfaces;
using QuizGen.DAL.Interfaces;
using System.Text;
using QuestPDF.Fluent;
using QuizGen.BLL.Services.Documents;

namespace QuizGen.BLL.Services;

public class QuizExportService : IQuizExportService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;

    public QuizExportService(
        IQuizRepository quizRepository,
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository)
    {
        _quizRepository = quizRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
    }

    public async Task<ServiceResult<byte[]>> ExportAsPdfAsync(int quizId, bool includeAnswers)
    {
        try
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz == null)
                return ServiceResult<byte[]>.CreateError("Quiz not found");

            var questions = (await _questionRepository.GetByQuizIdAsync(quizId)).ToList();
            if (questions == null || !questions.Any())
                return ServiceResult<byte[]>.CreateError("No questions found for this quiz");

            var answers = (await _answerRepository.GetByQuizIdAsync(quizId)).ToList();
            if (answers == null)
                return ServiceResult<byte[]>.CreateError("Failed to load quiz answers");

            var document = new QuizDocument(quiz, questions, answers, includeAnswers);
            var pdfBytes = document.GeneratePdf();

            return ServiceResult<byte[]>.CreateSuccess(pdfBytes);
        }
        catch (Exception ex)
        {
            return ServiceResult<byte[]>.CreateError($"Failed to export quiz as PDF: {ex.Message}");
        }
    }

    public async Task<ServiceResult<byte[]>> ExportAsTextAsync(int quizId, bool includeAnswers)
    {
        try
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            if (quiz == null)
                return ServiceResult<byte[]>.CreateError("Quiz not found");

            var questions = await _questionRepository.GetByQuizIdAsync(quizId);
            if (questions == null || !questions.Any())
                return ServiceResult<byte[]>.CreateError("No questions found for this quiz");

            var answers = await _answerRepository.GetByQuizIdAsync(quizId);
            if (answers == null)
                return ServiceResult<byte[]>.CreateError("Failed to load quiz answers");

            var sb = new StringBuilder();
            
            // Add quiz header
            sb.AppendLine($"Quiz: {quiz.Name}");
            sb.AppendLine($"Topic: {quiz.Prompt}");
            sb.AppendLine($"Difficulty: {quiz.Difficulty}");
            sb.AppendLine($"Created: {quiz.CreatedAt:g}");
            sb.AppendLine();

            // Add questions and answers
            for (int i = 0; i < questions.Count(); i++)
            {
                var question = questions.ElementAt(i);
                var questionAnswers = answers.Where(a => a.QuestionId == question.Id).ToList();

                sb.AppendLine($"{i + 1}. {question.Text}");
                
                foreach (var answer in questionAnswers)
                {
                    var prefix = includeAnswers && answer.IsCorrect ? "* " : "- ";
                    sb.AppendLine($"   {prefix}{answer.Text}");
                }

                if (includeAnswers && !string.IsNullOrEmpty(question.Explanation))
                {
                    sb.AppendLine($"\nExplanation: {question.Explanation}\n");
                }

                sb.AppendLine();
            }

            return ServiceResult<byte[]>.CreateSuccess(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        catch (Exception ex)
        {
            return ServiceResult<byte[]>.CreateError($"Failed to export quiz as text: {ex.Message}");
        }
    }
} 