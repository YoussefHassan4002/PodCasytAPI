# Deployment Checklist

## Pre-Deployment

- [ ] All code committed to GitHub
- [ ] Solution builds successfully (`dotnet build`)
- [ ] All tests pass (if applicable)
- [ ] Database migrations created
- [ ] `appsettings.Production.json` template created (without secrets)
- [ ] `.gitignore` configured correctly

## Database Setup

- [ ] SQL Server database created on MonsterASP
- [ ] Connection string obtained
- [ ] Database migrations ready to run
- [ ] Backup strategy planned

## Configuration

- [ ] Production connection string configured
- [ ] JWT key generated (strong, random, 32+ characters)
- [ ] CORS origins set to production frontend domain
- [ ] Environment variables planned
- [ ] Logging configuration reviewed

## Security

- [ ] JWT key is strong and secure
- [ ] Connection strings not in source control
- [ ] CORS restricted to production domain
- [ ] Swagger disabled or protected for production
- [ ] Hangfire dashboard authorization implemented
- [ ] HTTPS/SSL configured

## Deployment Steps

- [ ] GitHub repository connected to MonsterASP
- [ ] Build configuration verified
- [ ] Publish path set correctly (`Podcast.Api`)
- [ ] Database migrations executed
- [ ] Environment variables set in MonsterASP panel
- [ ] Application pool configured correctly

## Post-Deployment Verification

- [ ] Health check endpoint works: `/health`
- [ ] API endpoints respond correctly
- [ ] Authentication works (register/login)
- [ ] Database operations work
- [ ] RSS sync job scheduled in Hangfire
- [ ] Logging working (check log files)
- [ ] CORS working with frontend

## Monitoring

- [ ] Application logs accessible
- [ ] Error tracking configured
- [ ] Performance monitoring set up
- [ ] Database monitoring enabled

## Documentation

- [ ] API documentation updated
- [ ] Deployment process documented
- [ ] Troubleshooting guide created
- [ ] Team notified of deployment

