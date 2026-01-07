using Podcast.Core.Entities;
using Entities = Podcast.Core.Entities;

namespace Podcast.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Entities.Podcast> Podcasts { get; }
    IRepository<Episode> Episodes { get; }
    IRepository<Subscription> Subscriptions { get; }
    IRepository<PlayProgress> PlayProgresses { get; }
    IRepository<Playlist> Playlists { get; }
    IRepository<PlaylistItem> PlaylistItems { get; }
    IRepository<Rating> Ratings { get; }
    IRepository<Comment> Comments { get; }
    IRepository<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

