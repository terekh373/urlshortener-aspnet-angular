USE UrlShortenerDb;
GO

-- Short urls
DELETE FROM ShortenedUrls;

-- Users with roles
DELETE FROM AspNetUserRoles;

-- User tokens and claims
DELETE FROM AspNetUserTokens;
DELETE FROM AspNetUserClaims;
DELETE FROM AspNetUserLogins;

-- Role claims
DELETE FROM AspNetRoleClaims;

-- Users and roles
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;

GO

-- Checking if everything is clear
SELECT 'AspNetUsers' as TableName, COUNT(*) as Records FROM AspNetUsers
UNION ALL
SELECT 'AspNetRoles', COUNT(*) FROM AspNetRoles
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM AspNetUserRoles
UNION ALL
SELECT 'ShortenedUrls', COUNT(*) FROM ShortenedUrls;