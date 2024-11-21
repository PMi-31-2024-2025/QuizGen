namespace QuizGen.BLL.Models.Base;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public static ServiceResult<T> CreateSuccess(T data, string message = null)
    {
        return new ServiceResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ServiceResult<T> CreateError(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message
        };
    }
}