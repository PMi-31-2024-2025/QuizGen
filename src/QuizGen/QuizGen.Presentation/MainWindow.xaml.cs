using QuizGen.BLL;
using QuizGen.DAL;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace QuizGen.Presentation
{
    public partial class MainWindow : Window
    {
        private readonly UserService _userService;

        public MainWindow()
        {
            InitializeComponent();

            // Конфігурація DbContext для прикладу
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Database=quizgen;Username=postgres;Password=1111")
                .Options;

            var context = new AppDbContext(options);
            _userService = new UserService(context);
        }

        private async void LoadUsers_Click(object sender, RoutedEventArgs e)
        {
            var users = await _userService.GetAllUsersAsync();
            UsersListBox.ItemsSource = users.Select(u => $"{u.Id}: {u.Name}");
        }
    }
}