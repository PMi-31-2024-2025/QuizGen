namespace QuizGen.DAL.Context;

using Microsoft.EntityFrameworkCore;
using QuizGen.DAL.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<QuizTry> QuizTries { get; set; }
    public DbSet<QuizAnswer> QuizAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure default schema
        _ = modelBuilder.HasDefaultSchema("public");

        // User configuration
        _ = modelBuilder.Entity<User>(entity =>
        {
            _ = entity.ToTable("users");
            _ = entity.HasKey(e => e.Id);
            _ = entity.HasIndex(e => e.Username).IsUnique();

            _ = entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            _ = entity.Property(e => e.Username).HasColumnName("username").IsRequired();
            _ = entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            _ = entity.Property(e => e.OpenAiApiKey).HasColumnName("openai_api_key");
            _ = entity.Property(e => e.GptModel).HasColumnName("gpt_model")
                .HasDefaultValue("gpt-4o-mini");
        });

        // Quiz configuration
        _ = modelBuilder.Entity<Quiz>(entity =>
        {
            _ = entity.ToTable("quizzes");
            _ = entity.HasKey(e => e.Id);

            _ = entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.Property(e => e.AuthorId).HasColumnName("author_id").IsRequired();
            _ = entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            _ = entity.Property(e => e.Prompt).HasColumnName("prompt").IsRequired();
            _ = entity.Property(e => e.Difficulty).HasColumnName("difficulty").IsRequired();
            _ = entity.Property(e => e.NumQuestions).HasColumnName("num_questions").IsRequired();
            _ = entity.Property(e => e.AllowedTypes).HasColumnName("allowed_types").IsRequired();

            _ = entity.HasOne(q => q.Author)
                  .WithMany(u => u.Quizzes)
                  .HasForeignKey(q => q.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Question configuration
        _ = modelBuilder.Entity<Question>(entity =>
        {
            _ = entity.ToTable("questions");
            _ = entity.HasKey(e => e.Id);

            _ = entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.Property(e => e.QuizId).HasColumnName("quiz_id").IsRequired();
            _ = entity.Property(e => e.Text).HasColumnName("text").IsRequired();
            _ = entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            _ = entity.Property(e => e.Explanation).HasColumnName("explanation");

            _ = entity.HasOne(q => q.Quiz)
                  .WithMany(qz => qz.Questions)
                  .HasForeignKey(q => q.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Answer configuration
        _ = modelBuilder.Entity<Answer>(entity =>
        {
            _ = entity.ToTable("answers");
            _ = entity.HasKey(e => e.Id);

            _ = entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.Property(e => e.QuestionId).HasColumnName("question_id").IsRequired();
            _ = entity.Property(e => e.Text).HasColumnName("text").IsRequired();
            _ = entity.Property(e => e.IsCorrect).HasColumnName("is_correct").IsRequired();

            _ = entity.HasOne(a => a.Question)
                  .WithMany(q => q.Answers)
                  .HasForeignKey(a => a.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // QuizTry configuration
        _ = modelBuilder.Entity<QuizTry>(entity =>
        {
            _ = entity.ToTable("quiz_tries");
            _ = entity.HasKey(e => e.Id);

            _ = entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.Property(e => e.QuizId).HasColumnName("quiz_id").IsRequired();
            _ = entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            _ = entity.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
            _ = entity.Property(e => e.FinishedAt).HasColumnName("finished_at");

            _ = entity.HasOne(qt => qt.Quiz)
                  .WithMany(q => q.QuizTries)
                  .HasForeignKey(qt => qt.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);

            _ = entity.HasOne(qt => qt.User)
                  .WithMany(u => u.QuizTries)
                  .HasForeignKey(qt => qt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // QuizAnswer configuration
        _ = modelBuilder.Entity<QuizAnswer>(entity =>
        {
            _ = entity.ToTable("quiz_answers");
            _ = entity.HasKey(e => e.Id);

            _ = entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            _ = entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = entity.Property(e => e.QuizTryId).HasColumnName("quiz_try_id").IsRequired();
            _ = entity.Property(e => e.QuestionId).HasColumnName("question_id").IsRequired();
            _ = entity.Property(e => e.AnswerId).HasColumnName("answer_id").IsRequired();

            _ = entity.HasOne(qa => qa.QuizTry)
                  .WithMany(qt => qt.QuizAnswers)
                  .HasForeignKey(qa => qa.QuizTryId)
                  .OnDelete(DeleteBehavior.Cascade);

            _ = entity.HasOne(qa => qa.Question)
                  .WithMany(q => q.QuizAnswers)
                  .HasForeignKey(qa => qa.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            _ = entity.HasOne(qa => qa.Answer)
                  .WithMany(a => a.QuizAnswers)
                  .HasForeignKey(qa => qa.AnswerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}