# Podcast Backend API

A comprehensive ASP.NET Core Web API backend for a podcast application, designed to be hosted on MonsterASP.net.

## Architecture

The solution follows a clean architecture pattern with three main projects:

- **Podcast.Api**: ASP.NET Core Web API project containing controllers and API configuration
- **Podcast.Core**: Core domain entities, DTOs, and interfaces
- **Podcast.Infrastructure**: Data access layer (EF Core), repositories, and service implementations

## Features

- ✅ JWT Authentication
- ✅ User Registration & Login
- ✅ Podcast & Episode Management
- ✅ RSS Feed Ingestion & Syncing
- ✅ User Subscriptions
- ✅ Playback Progress Tracking
- ✅ Playlists Management
- ✅ Background Jobs (Hangfire) for RSS Sync
- ✅ Swagger/OpenAPI Documentation
- ✅ Health Checks
- ✅ Global Exception Handling
- ✅ Structured Logging (Serilog)
- ✅ CORS Configuration
- ✅ Rate Limiting Ready

## Database Schema

The application uses SQL Server with Entity Framework Core. Key entities include:

- **Users**: User accounts with email/password authentication
- **Podcasts**: Podcast metadata and RSS feed information
- **Episodes**: Individual podcast episodes with audio URLs
- **Subscriptions**: User podcast subscriptions
- **PlayProgress**: User playback progress tracking
- **Playlists**: User-created playlists
- **Ratings**: Podcast/episode ratings
- **Comments**: Episode comments
- **RefreshTokens**: JWT refresh token management

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB for development, SQL Server for production)
- Visual Studio 2022 or VS Code

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Podcast_Backend
   ```

2. **Update Connection String**
   Edit `Podcast.Api/appsettings.json` and update the `ConnectionStrings:DefaultConnection` with your SQL Server connection string.

3. **Update JWT Key**
   Edit `Podcast.Api/appsettings.json` and set a strong secret key in `Jwt:Key` (at least 32 characters).

4. **Run Database Migrations**
   ```bash
   cd Podcast.Api
   dotnet ef migrations add InitialCreate --project ../Podcast.Infrastructure
   dotnet ef database update
   ```

5. **Run the Application**
   ```bash
   dotnet run --project Podcast.Api
   ```

6. **Access Swagger UI**
   Navigate to `https://localhost:7156/swagger` (or the port shown in your terminal)

### Production Deployment (MonsterASP.net)

1. **Update Production Settings**
   - Edit `appsettings.Production.json`
   - Set your production SQL Server connection string
   - Set a strong JWT key (use environment variables in production)
   - Update CORS allowed origins to your frontend domain

2. **Database Migration**
   - Option A: Run migrations manually on the server
   - Option B: Enable automatic migrations on startup (not recommended for production)

3. **Deploy to MonsterASP.net**
   - Connect your GitHub repository to MonsterASP
   - Configure the connection string in MonsterASP control panel
   - Set environment variables for sensitive data (JWT key, etc.)
   - Deploy and verify the `/health` endpoint

4. **Hangfire Dashboard**
   - Access at `/hangfire` (protect this endpoint in production!)
   - Configure proper authorization for production

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token

### Podcasts
- `GET /api/podcasts` - Get paginated list of podcasts (supports search, category filters)
- `GET /api/podcasts/{id}` - Get podcast details
- `GET /api/podcasts/{id}/episodes` - Get episodes for a podcast

### Episodes
- `GET /api/episodes/{id}` - Get episode details (requires authentication)

### User Library (Requires Authentication)
- `POST /api/me/subscriptions/{podcastId}` - Subscribe to a podcast
- `DELETE /api/me/subscriptions/{podcastId}` - Unsubscribe from a podcast
- `GET /api/me/subscriptions` - Get user's subscriptions
- `PUT /api/me/progress/{episodeId}` - Update playback progress
- `GET /api/me/progress/{episodeId}` - Get playback progress

### Playlists (Requires Authentication)
- `POST /api/me/playlists` - Create a playlist
- `GET /api/me/playlists` - Get user's playlists
- `POST /api/me/playlists/{playlistId}/items` - Add episode to playlist
- `DELETE /api/me/playlists/{playlistId}/items/{episodeId}` - Remove episode from playlist

### Admin (Requires Authentication)
- `POST /api/admin/podcasts` - Add a new podcast
- `POST /api/admin/podcasts/{id}/sync` - Manually sync podcast RSS feed

### Import
- `POST /api/podcastsimport/import` - Import podcast from RSS URL (requires authentication)

## RSS Feed Syncing

The application includes automatic RSS feed syncing via Hangfire:

- **Automatic Sync**: Runs every hour at minute 30
- **Manual Sync**: Use admin endpoint or Hangfire dashboard
- **Import**: Users can import podcasts via RSS URL

## Configuration

### JWT Settings
```json
{
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "PodcastApi",
    "Audience": "PodcastApi"
  }
}
```

### CORS Settings
```json
{
  "Cors": {
    "AllowedOrigins": "http://localhost:3000,https://yourdomain.com"
  }
}
```

## Security Considerations

1. **JWT Key**: Use a strong, randomly generated key in production (at least 32 characters)
2. **Connection String**: Never commit production connection strings to source control
3. **CORS**: Restrict allowed origins to your frontend domain only
4. **Swagger**: Disable or protect Swagger UI in production
5. **Hangfire Dashboard**: Add proper authorization before deploying
6. **Rate Limiting**: Configure rate limiting for auth endpoints
7. **HTTPS**: Always use HTTPS in production

## Logging

The application uses Serilog for structured logging:
- Console output for development
- File logging to `logs/podcast-{date}.txt`
- Configurable log levels in `appsettings.json`

## Health Checks

Access the health check endpoint at `/health` to verify the application and database connectivity.

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string format
- Ensure database exists or migrations have run

### JWT Authentication Fails
- Verify JWT key is set correctly
- Check token expiration
- Ensure Authorization header format: `Bearer {token}`

### RSS Sync Fails
- Verify RSS URL is accessible
- Check network connectivity
- Review logs for specific error messages

## License

[Your License Here]

## Support

For issues and questions, please contact [your contact information].

