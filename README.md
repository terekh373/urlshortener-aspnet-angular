# 🔗 URL Shortener

A full-stack URL Shortener application.

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 Web API |
| Frontend | Angular 17 (standalone), Razor Pages |
| Database | SQL Server LocalDB via Entity Framework Core (Code First) |
| Authentication | ASP.NET Core Identity + JWT |
| Tests | xUnit + Moq |

---

## ✨ Features

- Shorten any URL to a unique 6-character code
- Redirect to original URL via short link (e.g. `/s/aB3xZ1`)
- Role-based access control: **Admin** and **User**
- Admin can delete all URLs, users can only delete their own
- Duplicate URL detection with error message
- URL existence validation before shortening
- Real-time table updates without page reload (Angular)
- Click counter — tracks how many times a short link was visited
- Copy short URL to clipboard with one click
- Search and filter URLs in real-time
- Sortable table columns
- About page with algorithm description, editable by Admin only
- Unit tests for repository and controller layers

---

## 📁 Project Structure

```
UrlShortener/
├── UrlShortener.API/            # ASP.NET Core Web API + Razor Pages
│   ├── Controllers/             # AuthController, UrlsController, RedirectController
│   ├── Pages/                   # Login, Register, About, Details (Razor)
│   └── Program.cs               # DI, Identity, JWT, CORS configuration
├── UrlShortener.Core/           # Business entities and interfaces
│   ├── Entities/                # AppUser, ShortenedUrl
│   └── Interfaces/              # IUrlRepository
├── UrlShortener.Infrastructure/ # EF Core implementation
│   ├── Data/                    # AppDbContext
│   └── Repositories/            # UrlRepository
├── UrlShortener.Tests/          # xUnit + Moq unit tests
│   ├── Controllers/             # UrlsControllerTests
│   └── Repositories/            # UrlRepositoryTests
└── urlshortener-angular/        # Angular 17 SPA
    └── src/app/
        ├── components/          # url-table, add-url, login, navbar
        ├── core/                # services, interceptors
        └── models/              # ShortenedUrl interface
```

---

## 🚀 How to Run

### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- SQL Server LocalDB (included with Visual Studio)
- Angular CLI:
```bash
npm install -g @angular/cli
```

---

### Step 1 — Run the Backend

1. Open `UrlShortener.sln` in **Visual Studio 2022**
2. Open **Package Manager Console**:
   `Tools` → `NuGet Package Manager` → `Package Manager Console`
3. Apply database migrations:
```powershell
Update-Database -Project UrlShortener.Infrastructure -StartupProject UrlShortener.API
```
4. Press **F5** to start the API

The API will be available at:
```
https://localhost:7150
```

Swagger UI:
```
https://localhost:7150/swagger
```

> On first launch, the app automatically creates roles (`Admin`, `User`) and a default admin account.

**Default admin credentials:**
```
Username: admin
Password: admin123
```

---

### Step 2 — Run the Frontend

Open a terminal in the `urlshortener-angular/` folder:

```bash
npm install
ng serve
```

The Angular app will be available at:
```
http://localhost:4200
```

---

## 🌐 Pages Overview

| Page | URL | Access |
|---|---|---|
| URL Table (Angular) | `http://localhost:4200` | Everyone |
| Login (Angular) | `http://localhost:4200/login` | Everyone |
| Register | `https://localhost:7150/register` | Everyone |
| About | `https://localhost:7150/about` | Everyone (edit: Admin only) |
| URL Details | `https://localhost:7150/urls/details/{id}` | Authorized only |
| Swagger | `https://localhost:7150/swagger` | Development only |

> ⚠️ **Important:** Always use `http://localhost:4200/login` to log in — not the Razor login page. The Angular login saves the JWT token to `localStorage` which the Angular app reads.

---

## 🧪 How to Test Manually

### 1. Anonymous user
- Open `http://localhost:4200` — table is visible, no Add form, no Delete buttons
- Open `https://localhost:7150/urls/details/1` — redirected (401)
- Open `https://localhost:7150/about` — visible, no Edit button

### 2. Register a new user
- Open `https://localhost:7150/register`
- Fill in Username, Email, Password → click Register

### 3. Login as regular user
- Open `http://localhost:4200/login` ← **use this, not the Razor page**
- Enter credentials → after login, Add New URL form appears

### 4. Add URLs
- Valid URL → added to table instantly without page reload
- Invalid format (e.g. `just-text`) → error message
- Unreachable URL → error "URL is not reachable"
- Duplicate URL → error "This URL already exists"

### 5. Short link redirect
- Click a short URL → redirected to original site
- Click count increments automatically

### 6. Login as Admin
- Clear localStorage first: open DevTools (F12) → Console → `localStorage.clear()`
- Open `http://localhost:4200/login` → enter `admin` / `admin123`
- Delete buttons visible for **all** records
- `https://localhost:7150/about` → Edit Description button visible

### 7. Run unit tests
- In Visual Studio: `Test` → `Run All Tests`
- All tests should be green ✅

---

## 🗄️ Database

### Connect via SSMS

Open **SQL Server Management Studio** and connect with:

```
Server type:    Database Engine
Server name:    (localdb)\MSSQLLocalDB
Authentication: Windows Authentication
```

### Useful SQL Queries

```sql
USE UrlShortenerDb;
GO

-- View all users
SELECT * FROM AspNetUsers;

-- View all roles
SELECT * FROM AspNetRoles;

-- View all shortened URLs
SELECT * FROM ShortenedUrls;

-- View users with their roles
SELECT u.UserName, r.Name AS Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON r.Id = ur.RoleId;
```

### Reset database (clear all data for fresh testing)

```sql
USE UrlShortenerDb;
GO

-- Delete in correct order (dependent tables first)
DELETE FROM ShortenedUrls;
DELETE FROM AspNetUserRoles;
DELETE FROM AspNetUserTokens;
DELETE FROM AspNetUserClaims;
DELETE FROM AspNetUserLogins;
DELETE FROM AspNetRoleClaims;
DELETE FROM AspNetUsers;
DELETE FROM AspNetRoles;
GO

-- Verify everything is empty
SELECT 'AspNetUsers'    AS TableName, COUNT(*) AS Records FROM AspNetUsers
UNION ALL
SELECT 'AspNetRoles',    COUNT(*) FROM AspNetRoles
UNION ALL
SELECT 'AspNetUserRoles', COUNT(*) FROM AspNetUserRoles
UNION ALL
SELECT 'ShortenedUrls',  COUNT(*) FROM ShortenedUrls;
```

> ⚠️ After resetting, **restart the API** (F5 in Visual Studio) — it will automatically recreate roles and the default `admin` account.

---

## 🔐 Shortening Algorithm

The algorithm generates a random **6-character code** from 62 possible characters:
- Lowercase letters: `a-z` (26)
- Uppercase letters: `A-Z` (26)
- Digits: `0-9` (10)

This gives **62⁶ = 56,800,235,584** unique combinations (~56 billion).

Before saving, each generated code is checked for uniqueness in the database. The original URL is also validated:
1. **Format check** — must start with `http://` or `https://`
2. **Existence check** — a HEAD request is sent to verify the URL actually responds

---

## ✅ Unit Tests

| Test | Description |
|---|---|
| `AddAsync_ShouldAddUrl` | Record is saved to database |
| `GetByIdAsync_ShouldReturnCorrectUrl` | Finds record by id |
| `GetByOriginalUrlAsync_ShouldReturnUrl_WhenExists` | Finds by original URL |
| `GetByOriginalUrlAsync_ShouldReturnNull_WhenNotExists` | Returns null if not found |
| `DeleteAsync_ShouldRemoveUrl` | Record is deleted |
| `GetAll_ShouldReturnOk` | Returns 200 OK |
| `Create_ShouldReturnBadRequest_WhenUrlAlreadyExists` | Duplicate is rejected |
| `Create_ShouldReturnBadRequest_WhenUrlFormatIsInvalid` | Invalid format rejected |
| `Delete_ShouldReturnForbid_WhenUserDeletesOthersUrl` | Cannot delete someone else's URL |
| `Delete_ShouldReturnNoContent_WhenAdminDeletesAnyUrl` | Admin can delete any URL |
| `Delete_ShouldReturnNotFound_WhenUrlDoesNotExist` | Returns 404 if not found |