using QuizGen.BLL.Models.Base;

namespace QuizGen.BLL.Services.Interfaces;

public interface IQuizExportService
{
    Task<ServiceResult<byte[]>> ExportAsPdfAsync(int quizId, bool includeAnswers);
    Task<ServiceResult<byte[]>> ExportAsTextAsync(int quizId, bool includeAnswers);
    Task<ServiceResult<byte[]>> ExportTryAsPdfAsync(int quizTryId);
    Task<ServiceResult<byte[]>> ExportTryAsTextAsync(int quizTryId);
}