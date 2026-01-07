# Quick Start Guide - Testing Your API

## 🎉 Your API is Running!

The API is now accessible at:
- **Swagger UI**: `https://localhost:7156/swagger` or `http://localhost:5073/swagger`
- **Health Check**: `https://localhost:7156/health`

## Step-by-Step Testing Guide

### 1. Test Health Endpoint ✅
Open: `http://localhost:5073/health`
- Should return: `{"status":"Healthy"}` or similar

### 2. Register a New User 👤

**Endpoint**: `POST /api/auth/register`

In Swagger UI:
1. Find `POST /api/auth/register`
2. Click "Try it out"
3. Use this test data:
```json
{
  "email": "test@example.com",
  "password": "Test123!@#"
}
```
4. Click "Execute"
5. **Save the token** from the response - you'll need it!

**Expected Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "...",
  "expiresAt": "2026-01-08T...",
  "user": {
    "id": 1,
    "email": "test@example.com"
  }
}
```

### 3. Test Login 🔐

**Endpoint**: `POST /api/auth/login`

Use the same credentials:
```json
{
  "email": "test@example.com",
  "password": "Test123!@#"
}
```

### 4. Add Authentication to Swagger 🔑

1. Click the **"Authorize"** button at the top of Swagger UI
2. In the "Value" field, enter: `Bearer YOUR_TOKEN_HERE`
   - Replace `YOUR_TOKEN_HERE` with the token from step 2
3. Click "Authorize"
4. Click "Close"

Now you can test authenticated endpoints!

### 5. Test Podcast Endpoints 📻

**Get All Podcasts**:
- `GET /api/podcasts`
- Try with query parameters:
  - `?page=1&pageSize=10`
  - `?search=tech`
  - `?category=technology`

### 6. Import a Podcast from RSS 🔗

**Endpoint**: `POST /api/podcastsimport/import`

**Note**: You need to be authenticated (use the token from step 2)

Try with a real podcast RSS feed:
```json
{
  "rssUrl": "https://feeds.npr.org/510289/podcast.xml"
}
```

Or try:
- `https://feeds.bbci.co.uk/programmes/b006qykl/rss.xml`
- `https://rss.cnn.com/rss/edition.rss`

### 7. Test Subscriptions 📚

**Subscribe to a Podcast**:
- `POST /api/me/subscriptions/{podcastId}`
- Replace `{podcastId}` with an ID from step 5

**Get Your Subscriptions**:
- `GET /api/me/subscriptions`

### 8. Test Playlists 🎵

**Create a Playlist**:
- `POST /api/me/playlists`
```json
{
  "name": "My Favorite Episodes"
}
```

**Get Your Playlists**:
- `GET /api/me/playlists`

### 9. Test Admin Endpoints (Optional) 🔧

**Add a Podcast Manually**:
- `POST /api/admin/podcasts`
```json
{
  "title": "My Podcast",
  "author": "John Doe",
  "description": "A great podcast",
  "rssUrl": "https://example.com/feed.xml"
}
```

**Sync a Podcast**:
- `POST /api/admin/podcasts/{id}/sync`

## 🎯 What to Do Next

### Immediate Next Steps:

1. **Test All Endpoints** ✅
   - Go through each endpoint in Swagger
   - Verify they work as expected

2. **Add Some Test Data** 📊
   - Import a few podcasts via RSS
   - Create subscriptions
   - Test playlists

3. **Check Hangfire Dashboard** ⏰
   - Visit: `http://localhost:5073/hangfire`
   - See scheduled jobs for RSS syncing

4. **Review Logs** 📝
   - Check `logs/` folder for application logs
   - Verify no errors

### Development Next Steps:

1. **Connect Your Flutter App** 📱
   - Update your Flutter app's API base URL
   - Test authentication flow
   - Test API calls

2. **Customize Configuration** ⚙️
   - Update CORS origins for your frontend
   - Adjust JWT expiration times if needed
   - Configure logging levels

3. **Add More Features** (Optional) 🚀
   - Ratings/Reviews endpoints
   - Comments endpoints
   - Search improvements
   - Pagination enhancements

### Production Deployment:

1. **Review SETUP.md** for deployment instructions
2. **Update appsettings.Production.json** with production values
3. **Push to GitHub**
4. **Deploy to MonsterASP.net**

## 🐛 Troubleshooting

### If endpoints return 401 Unauthorized:
- Make sure you clicked "Authorize" in Swagger
- Check that your token is still valid (24 hours)
- Try logging in again to get a new token

### If database errors occur:
- Verify LocalDB is running
- Check connection string in `appsettings.json`
- Run migrations: `dotnet ef database update --project Podcast.Infrastructure`

### If RSS import fails:
- Check the RSS URL is valid and accessible
- Some RSS feeds may require specific headers
- Check logs for detailed error messages

## 📚 API Endpoints Summary

### Public Endpoints (No Auth):
- `POST /api/auth/register` - Register user
- `POST /api/auth/login` - Login
- `GET /api/podcasts` - List podcasts
- `GET /api/podcasts/{id}` - Get podcast
- `GET /api/podcasts/{id}/episodes` - Get episodes

### Authenticated Endpoints (Require Token):
- `GET /api/episodes/{id}` - Get episode
- `POST /api/me/subscriptions/{podcastId}` - Subscribe
- `DELETE /api/me/subscriptions/{podcastId}` - Unsubscribe
- `GET /api/me/subscriptions` - Get subscriptions
- `PUT /api/me/progress/{episodeId}` - Update progress
- `GET /api/me/progress/{episodeId}` - Get progress
- `POST /api/me/playlists` - Create playlist
- `GET /api/me/playlists` - Get playlists
- `POST /api/me/playlists/{id}/items` - Add episode
- `DELETE /api/me/playlists/{id}/items/{episodeId}` - Remove episode
- `POST /api/podcastsimport/import` - Import RSS
- `POST /api/admin/podcasts` - Add podcast (admin)
- `POST /api/admin/podcasts/{id}/sync` - Sync podcast (admin)

## 🎉 You're All Set!

Your podcast API is fully functional. Start testing and integrating with your Flutter app!

