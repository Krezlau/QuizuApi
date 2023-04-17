using Azure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using QuizuApi.Models.Database;

namespace QuizuApi.Data
{
    public class QuizuApiDbContext : IdentityDbContext<User>
    {
        public QuizuApiDbContext(DbContextOptions<QuizuApiDbContext> options) : base(options) { }

        public override DbSet<User> Users { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizComment> QuizComments { get; set; }
        public DbSet<QuizLike> QuizLikes { get; set; }
        public DbSet<QuizPlay> QuizPlays { get; set; }
        public DbSet<QuizSettings> QuizSettings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quiz>()
                .HasIndex(q => q.Title)
                .IsUnique(true);

            base.OnModelCreating(modelBuilder);
        }

        private void TrackChanges()
        {
            var tracker = ChangeTracker;

            foreach (var entry in tracker.Entries())
            {
                if (entry.Entity is AuditModel)
                {
                    var referenceEntity = entry.Entity as AuditModel;
                    if (referenceEntity is not null)
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                referenceEntity.CreatedAt = DateTime.Now;
                                break;
                            case EntityState.Modified:
                                referenceEntity.LastModifiedAt = DateTime.Now;
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TrackChanges();

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            TrackChanges();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            TrackChanges();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            TrackChanges();

            return base.SaveChanges();
        }
    }
}