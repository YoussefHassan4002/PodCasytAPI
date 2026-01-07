using Microsoft.EntityFrameworkCore.Storage;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using Podcast.Infrastructure.Data;
using Entities = Podcast.Core.Entities;

namespace Podcast.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<User>? _users;
    private IRepository<Entities.Podcast>? _podcasts;
    private IRepository<Episode>? _episodes;
    private IRepository<Subscription>? _subscriptions;
    private IRepository<PlayProgress>? _playProgresses;
    private IRepository<Playlist>? _playlists;
    private IRepository<PlaylistItem>? _playlistItems;
    private IRepository<Rating>? _ratings;
    private IRepository<Comment>? _comments;
    private IRepository<RefreshToken>? _refreshTokens;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Entities.Podcast> Podcasts => _podcasts ??= new Repository<Entities.Podcast>(_context);
    public IRepository<Episode> Episodes => _episodes ??= new Repository<Episode>(_context);
    public IRepository<Subscription> Subscriptions => _subscriptions ??= new Repository<Subscription>(_context);
    public IRepository<PlayProgress> PlayProgresses => _playProgresses ??= new Repository<PlayProgress>(_context);
    public IRepository<Playlist> Playlists => _playlists ??= new Repository<Playlist>(_context);
    public IRepository<PlaylistItem> PlaylistItems => _playlistItems ??= new Repository<PlaylistItem>(_context);
    public IRepository<Rating> Ratings => _ratings ??= new Repository<Rating>(_context);
    public IRepository<Comment> Comments => _comments ??= new Repository<Comment>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

