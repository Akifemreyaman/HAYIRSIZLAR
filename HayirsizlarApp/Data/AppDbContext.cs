using Microsoft.EntityFrameworkCore;
using HayirsizlarApp.Models;

namespace HayirsizlarApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Tweet> Tweets { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.AvatarColor).HasMaxLength(7);
                entity.Property(u => u.MoodStatus).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(255);
            });

            // Configure Tweet entity
            modelBuilder.Entity<Tweet>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Content).IsRequired(false);
                entity.Property(t => t.MediaUrl).HasMaxLength(255);
                entity.Property(t => t.MediaType)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tweets)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.ParentTweet)
                      .WithMany(t => t.Replies)
                      .HasForeignKey(t => t.ParentTweetId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);

                entity.HasOne(n => n.ReceiverUser)
                      .WithMany()
                      .HasForeignKey(n => n.ReceiverUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.SenderUser)
                      .WithMany()
                      .HasForeignKey(n => n.SenderUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Tweet)
                      .WithMany()
                      .HasForeignKey(n => n.TweetId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
