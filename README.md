# SmartComply — Internal Auditing System

> A web-based internal auditing system developed for **Holistic Lab** to streamline compliance tracking, audit management, and stakeholder reporting.

---

## 📋 Table of Contents

- [About the Project](#about-the-project)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [System Architecture](#system-architecture)
- [Getting Started](#getting-started)
- [Deployment](#deployment)
- [Environment Configuration](#environment-configuration)
- [Database](#database)
- [Contributing](#contributing)

---

## About the Project

<img src="9e31a571-3e6d-4c86-81c0-d129079c828f.jpg" alt="SmartComply Logo" width="200"/>

**SmartComply** is an internal auditing system commissioned by **Holistic Lab** to digitise and centralise their audit and compliance processes. The system allows administrators and auditors to manage audit records, track compliance status, assign roles, and generate reports — all from a single web interface.

This project replaces manual audit tracking with a structured, role-based web application that ensures accountability and transparency across the organisation.

---

## Features

- 🔐 **Role-based access control** — Admin, Manager, and Auditor roles
- 📋 **Audit management** — Create, assign, and track audit tasks
- ✅ **Compliance tracking** — Monitor compliance status across departments
- 👥 **User management** — Admin-controlled user registration and role assignment
- 📊 **Dashboard** — Overview of audit progress and compliance metrics
- 🔒 **Secure authentication** — ASP.NET Core Identity with confirmed accounts

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 9) |
| Language | C# |
| Frontend | Razor Views, HTML, CSS, JavaScript |
| Authentication | ASP.NET Core Identity |
| ORM | Entity Framework Core |
| Database (Local) | SQL Server Express |
| Database (Production) | Azure SQL Database |
| Hosting | Azure App Service (Free F1 Tier) |
| CI/CD | GitHub Actions |
| Version Control | Git / GitHub |

---

## System Architecture

```
┌─────────────────────────────────────────────┐
│               GitHub Repository              │
│         (Source Code + CI/CD Workflows)      │
└───────────────────┬─────────────────────────┘
                    │ Push to main
                    ▼
┌─────────────────────────────────────────────┐
│              GitHub Actions                  │
│         (Build → Publish → Deploy)           │
└───────────────────┬─────────────────────────┘
                    │ Deploy
                    ▼
┌─────────────────────────────────────────────┐
│          Azure App Service (F1 Free)         │
│       smart-comply.azurewebsites.net         │
└───────────────────┬─────────────────────────┘
                    │ Connects to
                    ▼
┌─────────────────────────────────────────────┐
│            Azure SQL Database                │
│         (Production Database)                │
└─────────────────────────────────────────────┘
```

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or VS Code
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/akim730/smart-comply.git
cd smart-comply
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Set up local database**

Update `appsettings.json` with your local SQL Server connection string:
```json
"ConnectionStrings": {
    "Database": "Server=localhost\\SQLEXPRESS;Database=smartComply;Trusted_Connection=True;TrustServerCertificate=True"
}
```

4. **Apply migrations**
```bash
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

The app will be available at `https://localhost:7000` (or whichever port is configured).

---

## Deployment

SmartComply is hosted on **Microsoft Azure** using the following services:

### Azure App Service
- **Plan:** Free F1 Tier
- **Runtime:** .NET 9 on Windows
- **URL:** [SmartComply](https://smart-comply-hjetbab7fccffqbz.eastasia-01.azurewebsites.net/Identity/Account/Login?ReturnUrl=%2F)

### CI/CD with GitHub Actions
Every push to the `main` branch automatically triggers a deployment pipeline:

1. GitHub Actions checks out the code
2. Builds the project using `dotnet build`
3. Publishes the release build
4. Deploys to Azure App Service

The workflow file is located at `.github/workflows/main_smart-comply.yml`.

### Azure SQL Database
- Production database hosted on Azure SQL
- Migrations are applied automatically on startup via `context.Database.Migrate()`
- Connection string is stored securely in Azure App Service Configuration (never in source code)

---

## Environment Configuration

The app uses different database connections per environment:

| Environment | Database |
|---|---|
| Development | Local SQL Server Express |
| Production | Azure SQL Database |

Environment is controlled via the `ASPNETCORE_ENVIRONMENT` setting in Azure App Service Configuration.

### Required Azure App Service Settings

| Setting | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### Required Azure Connection Strings

| Name | Type |
|---|---|
| `Productiondb` | SQLAzure |

> ⚠️ Never commit production credentials to the repository. Always store secrets in Azure App Service Configuration.

---

## Database

The application uses **Entity Framework Core** with code-first migrations.

### Run migrations locally
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Default seeded roles
On first startup, the following roles and users are automatically seeded:
- **Admin** — Full system access, user management, and system configuration
- **Manager** — Oversee audit progress, review reports, and manage auditors
- **Auditor** — Create, manage, and submit audit records

---

## Contributing

This project is developed for internal use by **Holistic Lab**. For any changes or feature requests, please contact the development team.

---

## Developer

Developed by **Team ByteShift** — [github.com/akim730](https://github.com/akim730)

Stakeholder: **Holistic Lab**

---

*Last updated: June 2026*
