using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizGen.BLL.Models.Quiz;
using QuizGen.BLL.Models.QuizTry;
using QuizGen.BLL.Services.Interfaces;

namespace QuizGen.API.Pages
{
    public class QuizDetailsModel : PageModel
    {
        private readonly IQuizService _quizService;
        private readonly IQuizTryService _quizTryService;

        public QuizDetailsModel(
            IQuizService quizService, 
            IQuizTryService quizTryService)
        {
            _quizService = quizService;
            _quizTryService = quizTryService;
            ErrorMessage = string.Empty;
        }

        public string ErrorMessage { get; private set; }
        public QuizDto? Quiz { get; private set; }
        public IEnumerable<QuizTryDto> QuizTries { get; private set; } = Enumerable.Empty<QuizTryDto>();

        [BindProperty(SupportsGet = true)]
        public int QuizId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            var authToken = Request.Cookies["AuthToken"];
            var userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            if (QuizId <= 0)
            {
                ErrorMessage = "Quiz ID is required";
                return Page();
            }

            // Parse the user ID
            if (!int.TryParse(userId, out int currentUserId))
            {
                ErrorMessage = "Invalid user ID";
                return Page();
            }

            // Load the quiz and attempts
            await LoadQuizAndAttempts(currentUserId);

            // Check if the quiz was loaded successfully
            if (Quiz == null)
            {
                ErrorMessage = "Quiz not found";
                return Page();
            }

            // The user is authorized if they are the quiz author
            bool isAuthorized = Quiz.AuthorId == currentUserId;

            // If not the author, check if they have any attempts
            if (!isAuthorized && QuizTries != null)
            {
                isAuthorized = QuizTries.Any(t => t.UserId == currentUserId);
            }

            if (!isAuthorized)
            {
                ErrorMessage = "You are not authorized to view this quiz";
                return Page();
            }

            return Page();
        }

        // Helper method to load quiz and attempts and handle authorization
        private async Task LoadQuizAndAttempts(int currentUserId)
        {
            try
            {
                // Get the quiz
                var quizResult = await _quizService.GetQuizByIdAsync(QuizId);
                if (!quizResult.Success)
                {
                    ErrorMessage = quizResult.Message;
                    return;
                }

                Quiz = quizResult.Data;

                // Get the quiz attempts
                var quizTriesResult = await _quizTryService.GetQuizTriesByQuizAsync(QuizId);
                if (quizTriesResult.Success)
                {
                    QuizTries = quizTriesResult.Data;
                }
                else
                {
                    ErrorMessage = quizTriesResult.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while retrieving quiz details: {ex.Message}";
                if (ex.InnerException != null)
                {
                    ErrorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
            }
        }
    }
} 