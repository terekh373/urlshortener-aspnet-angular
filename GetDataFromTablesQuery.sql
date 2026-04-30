USE UrlShortenerDb;
GO

-- Users
SELECT * FROM AspNetUsers;

-- Roles
SELECT * FROM AspNetRoles;

-- Short urls
SELECT * FROM ShortenedUrls;

-- Users with roles
SELECT u.UserName, r.Name as Role 
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId;