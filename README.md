# 📚 Bookano — Library Management System

> A full-featured, role-based Library Management System built with ASP.NET Core MVC (.NET 10), following clean architecture principles.

🌐 **Live Demo:** [https://bookano.runasp.net](https://bookano.runasp.net)  
🔑 **Demo Credentials:** Username: `Admin` · Password: `P@ssword1504`

---

## Table of Contents

- [Overview](#overview)
- [Live Demo](#live-demo)
- [Features](#features)
- [Architecture](#architecture)
- [Domain Model](#domain-model)
- [Roles & Permissions](#roles--permissions)
- [Tech Stack](#tech-stack)
- [NuGet Packages](#nuget-packages)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Background Jobs](#background-jobs)
- [Notifications](#notifications)
- [Reports & Exports](#reports--exports)
- [Security](#security)
- [Project Structure](#project-structure)
- [License](#license)

---

## Overview

**Bookano** is a production-ready Library Management System designed to manage books, subscribers, rentals, and users through a clean web interface. It supports full CRUD operations, rental lifecycle management (borrow → extend → return), subscription tracking, automated daily notifications via Email and WhatsApp, and comprehensive reporting with Excel and PDF export.

---

## Live Demo

| Detail | Value |
|---|---|
| 🌐 URL | [https://bookano.runasp.net](https://bookano.runasp.net) |
| 👤 Username | `Admin` |
| 🔑 Password | `P@ssword1504` |

The admin account has full access to all modules (Admin + Archive + Reception roles).

---

## Features

### 📖 Book Management (Archive Role)
- Add, edit, view, and soft-delete books with cover images (Cloudinary or local storage)
- Manage multiple **Book Copies** per book with edition and serial number tracking
- ISBN uniqueness validation, publisher/author/category assignment
- DataTables-powered list with server-side filtering, searching, and pagination
- Toggle book availability for rental

### 👥 Subscriber Management (Reception Role)
- Full subscriber lifecycle: register, edit, blacklist/unblacklist
- ID photo upload (Cloudinary or local)
- Unique validations on National ID, mobile number, and email
- Location-based filtering by Governorate and Area (cascading dropdowns via AJAX)
- Subscription renewal with one click

### 📦 Rental Management (Reception Role)
- Create rentals by scanning book copy serial numbers
- Edit same-day rentals
- Return individual or all copies with penalty tracking
- Cancel rentals (same-day only)
- Eligibility checks: blacklist status, subscription validity, max-copies-held limit
- Rental extension support (within the allowed window)

### 📊 Dashboard (All Authenticated Users)
- Summary KPIs: total books, subscribers, rentals, available copies
- Interactive **Rentals per Day** chart with custom date-range filtering
- **Subscribers per Governorate** doughnut chart
- Recent activity feed

### 📑 Reports (Admin Role)
- **Books Report** — filter by author & category, paginated, export to **Excel** or **PDF**
- **Rentals Report** — filter by duration, export to **Excel** or **PDF**
- **Delayed Rentals Report** — overdue rentals with delay days, export to **Excel** or **PDF**

### 👤 User Management (Admin Role)
- Create/edit system users with role assignment
- Toggle user active/inactive status
- Admin password reset
- Unlock locked-out accounts
- Email confirmation flow on user creation

### 🔍 Public Book Search
- Auto-complete search for books (no authentication required)
- Public book detail page with cover, authors, categories, and description

### 🔔 Automated Notifications
- Daily Hangfire jobs (run at 12:00 PM UTC):
  - **Subscription expiring soon** (5 days before expiry) — Email + WhatsApp
  - **Subscription expired today** — Email + WhatsApp
  - **Book rental due tomorrow** — Email + WhatsApp

---

## Architecture

Bookano follows **Clean Architecture** with four separate projects:

```
Bookano/
├── Bookano.Domain          # Entities, enums, domain interfaces, business rules
├── Bookano.Application     # Use cases, DTOs, service interfaces, validators
├── Bookano.Infrastructure  # EF Core, Identity, repositories, email, Cloudinary, Hangfire
└── Bookano.Web             # ASP.NET Core MVC controllers, views, view models, filters
```

**Dependency direction:** `Web` → `Application` + `Infrastructure` → `Domain`

The domain layer has **zero external dependencies**. Business rules (rental eligibility, extension eligibility, subscriber status) live entirely in the domain entities.

---

## Domain Model

```
ApplicationUser (IdentityUser)
    ↕ (audit)

Subscriber
    ├── Subscriptions[]      (start/end date, renewable)
    ├── Rentals[]
    └── Area → Governorate

Rental
    └── RentalCopies[]
            └── BookCopy → Book
                            ├── Authors[]   (BookAuthor join)
                            ├── Categories[] (BookCategory join)
                            └── Publisher
```

### Key Domain Rules
| Rule | Value |
|---|---|
| Rental duration | **7 days** |
| Max rental duration (with extension) | **14 days** |
| Max concurrent copies per subscriber | **3 copies** |
| Extension window | Must be requested before day 7 |
| Blacklisted subscribers | Cannot rent or renew |

### Enums
| Enum | Values |
|---|---|
| `SubscriberStatus` | `Active`, `Inactive`, `Banned` |
| `SubscriptionStatus` | `Active`, `Expired` |
| `SubscriptionType` | (defined per subscription record) |
| `RentalEligibility` | `Eligible`, `BlackListed`, `Inactive`, `MaxCopiesReached` |
| `ExtensionEligibility` | `Eligible`, `SubscriberBlackListed`, `SubscriberInactive`, `NotAllowed` |

---

## Roles & Permissions

| Role | Access |
|---|---|
| **Admin** | Users management, Reports, Dashboard, full system access |
| **Archive** | Books management, Book Copies management |
| **Reception** | Subscribers management, Rentals management |

> The default seeded admin user holds **all three roles**.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 (SQL Server) |
| Auth | ASP.NET Core Identity |
| Background Jobs | Hangfire (SQL Server storage) |
| Image Storage | Cloudinary (primary) / Local filesystem (fallback) |
| Email | SMTP (Gmail) via custom `EmailSender` |
| WhatsApp | WhatsApp Cloud API (`WhatsAppCloudApi`) |
| Logging | Serilog (File + SQL Server sinks) |
| Mapping | AutoMapper |
| Validation | FluentValidation + ExpressiveAnnotations |
| PDF Export | OpenHtmlToPdf |
| Excel Export | ClosedXML |
| ID Obfuscation | Hashids.net |
| Data Protection | ASP.NET Core Data Protection |
| Client-side Tables | DataTables (server-side processing) |

---

## NuGet Packages

### Bookano.Application
| Package | Purpose |
|---|---|
| `AutoMapper` | DTO ↔ Domain mapping |
| `FluentValidation.DependencyInjectionExtensions` | Validation |
| `System.Linq.Dynamic.Core` | Dynamic LINQ for DataTables |

### Bookano.Infrastructure
| Package | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | EF Core SQL Server provider |
| `CloudinaryDotNet` | Cloud image hosting |
| `SixLabors.ImageSharp` | Local image processing/resizing |
| `Hangfire` | Recurring background jobs |
| `WhatsAppCloudApi` | WhatsApp Business notifications |
| `Serilog.AspNetCore` + sinks | Structured logging |
| `Newtonsoft.Json` | JSON serialization |

### Bookano.Web
| Package | Purpose |
|---|---|
| `FluentValidation.AspNetCore` | Server + client-side validation |
| `ClosedXML` | Excel report generation |
| `OpenHtmlToPdf.netcore` | PDF report generation |
| `Hashids.net` | Obfuscate integer IDs in URLs |
| `UoN.ExpressiveAnnotations.NetCore` | Conditional validation attributes |
| `AutoMapper` | View model mapping |
| `Serilog` sinks | Logging |
| `System.Linq.Dynamic.Core` | Dynamic DataTables ordering |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (local or remote)
- (Optional) Cloudinary account
- (Optional) Gmail SMTP credentials
- (Optional) WhatsApp Cloud API credentials

### Clone & Run

```bash
git clone https://github.com/AdhamAlkhateeb/bookano.git
cd bookano
```

#### 1. Configure secrets

Create a `appsettings.Development.json` or use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
cd Bookano.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=BookanoDB;Trusted_Connection=True;"
dotnet user-secrets set "CloudinarySettings:Cloud" "<your-cloud>"
dotnet user-secrets set "CloudinarySettings:APIKey" "<your-key>"
dotnet user-secrets set "CloudinarySettings:APISecret" "<your-secret>"
dotnet user-secrets set "MailSettings:Email" "<your-email>"
dotnet user-secrets set "MailSettings:Password" "<your-password>"
dotnet user-secrets set "WhatsAppConfigurations:PhoneNumberId" "<phone-number-id>"
dotnet user-secrets set "WhatsAppConfigurations:AccessToken" "<access-token>"
```

#### 2. Apply migrations

```bash
cd Bookano.Web
dotnet ef database update --project ../Bookano.Infrastructure
```

EF Core migrations are stored in `Bookano.Infrastructure/Persistence/Migrations`.

#### 3. Run the app

```bash
dotnet run --project Bookano.Web
```

The database seeder (`DatabaseInitializer.SeedAsync`) runs on startup and creates:
- Roles: `Admin`, `Archive`, `Reception`
- Default admin user:

| Field | Value |
|---|---|
| Username | `admin` |
| Email | `admin@bookano.com` |
| Password | `P@ssword1504` |

---

## Configuration

All settings live in `Bookano.Web/appsettings.json`. Sensitive values should be supplied via User Secrets or environment variables in production.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<SQL Server connection string>"
  },
  "CloudinarySettings": {
    "Cloud": "",
    "APIKey": "",
    "APISecret": ""
  },
  "MailSettings": {
    "Email": "",
    "DisplayName": "Bookano",
    "Password": "",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "TemplatesPath": "templates"
  },
  "WhatsAppConfigurations": {
    "PhoneNumberId": "",
    "AccessToken": ""
  }
}
```

### Image Storage

Bookano supports **keyed DI** for image services:
- `"cloudinary"` — uploads images to Cloudinary CDN
- `"local"` — saves images to `wwwroot/images/`

### Logging

Serilog is configured to write:
- **File sink** — rolling daily JSON and text logs in `./Logs/`
- **SQL Server sink** — error-level logs to `logging.Logs` table (includes `UserId` and `UserName` columns)

---

## Background Jobs

Hangfire recurring jobs run **daily at 12:00 PM UTC**:

| Job ID | Method | Trigger |
|---|---|---|
| `prepare-expiration-alerts` | `SubscriptionJobs.PrepareExpirationAlerts` | `0 12 * * *` |
| `prepare-rental-alerts` | `RentalJobs.SendExpiringSoonAlerts` | `0 12 * * *` |

The Hangfire dashboard is available at `/hangfire` (read-only, Admin role required).

---

## Notifications

### Email Notifications
- **Subscription expiring soon** (5 days before end date)
- **Subscription expired today**
- **Book rental due tomorrow** (lists all expiring book titles)

All emails use HTML templates stored in `wwwroot/templates/`.

### WhatsApp Notifications
- Sent via the **WhatsApp Cloud API** using registered message templates
- Only sent to subscribers with `HasWhatsApp = true`
- Templates: `SubscriptionExpiration`, `SubscriptionExpired`, `RentalExpiringSoon`

---

## Reports & Exports

| Report | Filters | Excel | PDF |
|---|---|---|---|
| Books | Authors, Categories | ✅ | ✅ |
| Rentals | Duration | ✅ | ✅ |
| Delayed Rentals | — | ✅ | ✅ |

PDF generation uses `OpenHtmlToPdf` by rendering Razor views to HTML first (`IViewRendererService`), then converting to PDF in landscape orientation.

Excel files are generated with `ClosedXML` and returned as `.xlsx` downloads with timestamped filenames.

---

## Security

| Feature | Details |
|---|---|
| Authentication | ASP.NET Core Identity with email confirmation |
| Authorization | Role-based (`Admin`, `Archive`, `Reception`) + policy-based (`AdminsOnly`) |
| CSRF Protection | Global `AutoValidateAntiforgeryTokenAttribute` on all MVC routes |
| Cookie Security | `CookieSecurePolicy.Always` (HTTPS-only cookies) |
| Clickjacking | `X-Frame-Options: Deny` header on all responses |
| HSTS | Enabled in production |
| Account Lockout | After **5 failed** login attempts |
| Security Stamp | Validated every **5 minutes** |
| Data Protection | `IDataProtectionProvider` used to obfuscate subscriber IDs in URLs |
| Concurrency | `RowVersion` (optimistic concurrency) on `Book` entity |
| Idempotency | `IdempotencyKey` on `Book` entity to prevent duplicate submissions |

---

## Project Structure

```
Bookano/
│
├── Bookano.Domain/
│   ├── Entities/           # Book, BookCopy, Subscriber, Rental, RentalCopy,
│   │                       #   Subscription, Author, Category, Publisher,
│   │                       #   Area, Governorate, ApplicationUser
│   ├── Enums/              # RentalEligibility, ExtensionEligibility,
│   │                       #   SubscriberStatus, SubscriptionStatus, SubscriptionType
│   ├── Common/
│   │   └── BaseEntity.cs   # Auditable base class
│   ├── Constants/          # AppRoles, RentalConstants
│   └── Interfaces/         # Domain repository contracts
│
├── Bookano.Application/
│   ├── Services/           # IBookService, ISubscriberService, IRentalService,
│   │                       #   IUserService, IDashboardService, IReportsService,
│   │                       #   ISearchService, IHomeService, IAreaService, ...
│   ├── DTOs/               # Request/response DTOs per feature
│   ├── Interfaces/         # IUnitOfWork, IEmailSender, IImageService, ...
│   ├── Common/             # Result<T>, DataTableQuery, PaginatedList
│   ├── Mappings/           # AutoMapper profiles
│   ├── Validators/         # FluentValidation validators
│   └── Constants/          # Application-level constants
│
├── Bookano.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/  # EF Core entity configurations
│   │   ├── Repositories/    # UnitOfWork + generic/specific repositories
│   │   ├── Interceptors/    # AuditableInterceptor (auto-fill CreatedBy/UpdatedBy)
│   │   ├── Migrations/      # EF Core migration files
│   │   └── Seeds/           # DefaultRoles, DefaultUsers, DatabaseInitializer
│   ├── Services/
│   │   ├── CloudinaryImageService.cs
│   │   ├── LocalImageService.cs
│   │   ├── EmailSender.cs
│   │   ├── EmailBodyBuilder.cs
│   │   ├── WhatsAppService.cs
│   │   ├── SubscriberNotificationService.cs
│   │   └── UserNotificationService.cs
│   ├── BackgroundServices/
│   │   ├── SubscriptionJobs.cs  # Expiration alerts
│   │   └── RentalJobs.cs        # Return-due alerts
│   ├── Identity/            # CurrentUserService, ApplicationUserClaimsPrincipalFactory
│   └── Settings/            # CloudinarySettings, MailSettings, WhatsAppSettings
│
└── Bookano.Web/
    ├── Controllers/         # HomeController, DashboardController, BooksController,
    │                        #   BookCopiesController, SubscribersController,
    │                        #   RentalsController, UsersController, ReportsController,
    │                        #   SearchController, AreasController, AuthorsController,
    │                        #   CategoriesController, PublishersController
    ├── Views/               # Razor views per controller
    ├── ViewModels/          # Strongly-typed view models per feature
    ├── Areas/Identity/      # Scaffolded Identity Razor Pages (login, register, etc.)
    ├── Services/
    │   ├── PDF/             # IPdfService, PdfService
    │   └── Export/          # IExcelService, ExcelService
    ├── Filters/             # AjaxOnlyAttribute, HangfireAuthorizationFilter
    ├── Validators/          # FluentValidation validators for view models
    ├── Mapping/             # MappingProfile (Web-layer AutoMapper)
    ├── Helpers/             # ApplicationUserClaimsPrincipalFactory
    ├── Binders/             # DataTableRequestBinder
    ├── Extensions/          # ClaimsPrincipal extensions (GetUserId)
    ├── Constants/           # ReportsConfigurations
    └── wwwroot/             # Static assets (CSS, JS, images, templates)
```

---

## License

## License

Copyright © 2026 Adham Alkhateeb

This repository is provided for portfolio, educational, and demonstration purposes only.

You are permitted to view, clone, and study the source code for personal learning and evaluation purposes.

You may not:

* Copy substantial portions of this project into another application.
* Redistribute this project or any modified version of it.
* Use this project or its source code in commercial products or services.
* Publish this project under another name.
* Claim this work, in whole or in part, as your own.

The software is provided "AS IS", without warranty of any kind, express or implied. The author shall not be liable for any claim, damages, or other liability arising from the use of this software.

For permissions beyond those granted above, written authorization from the author is required.
