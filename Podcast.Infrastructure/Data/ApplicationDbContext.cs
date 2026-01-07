using Microsoft.EntityFrameworkCore;
using Podcast.Core.Entities;
using Entities = Podcast.Core.Entities;

namespace Podcast.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Entities.Podcast> Podcasts { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PlayProgress> PlayProgresses { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistItem> PlaylistItems { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token);
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Podcast configuration
        modelBuilder.Entity<Entities.Podcast>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
        });

        // Episode configuration
        modelBuilder.Entity<Episode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AudioUrl).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Podcast)
                .WithMany(p => p.Episodes)
                .HasForeignKey(e => e.PodcastId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.PodcastId, e.EpisodeNumber, e.SeasonNumber });
        });

        // Subscription configuration
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.PodcastId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Podcast)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.PodcastId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlayProgress configuration
        modelBuilder.Entity<PlayProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.EpisodeId }).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.PlayProgresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Episode)
                .WithMany(ep => ep.PlayProgresses)
                .HasForeignKey(e => e.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Playlist configuration
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Playlists)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlaylistItem configuration
        modelBuilder.Entity<PlaylistItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlaylistId, e.Order });
            entity.HasOne(e => e.Playlist)
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Episode)
                .WithMany(ep => ep.PlaylistItems)
                .HasForeignKey(e => e.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Rating configuration
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Stars).IsRequired();
            entity.HasCheckConstraint("CK_Rating_Stars", "[Stars] >= 1 AND [Stars] <= 5");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Podcast)
                .WithMany(p => p.Ratings)
                .HasForeignKey(e => e.PodcastId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Episode)
                .WithMany(ep => ep.Comments)
                .HasForeignKey(e => e.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

