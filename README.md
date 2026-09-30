# Valav Boys Wallet

An ASP.NET Core MVC application for managing a group's wallet, member contributions, deposit and withdrawal requests, and administrator approvals.

## Stack

- .NET 9, ASP.NET Core MVC and Razor Pages
- ASP.NET Core Identity
- Entity Framework Core and SQL Server

## Run locally

1. Install the .NET 9 SDK and SQL Server or SQL Server Express.
2. Open `ValavBoysWallet.sln` in Visual Studio.
3. Use **Manage User Secrets** to configure `ConnectionStrings:DefaultConnection` for your local database.
4. For a new database, configure `BootstrapAdmin:Email` and `BootstrapAdmin:Password` privately. Use a unique password of at least 12 characters, including uppercase and lowercase letters and a number.
5. Build and run the project. Startup applies migrations, creates the roles and group wallet, and creates the administrator when no admin exists.

Existing administrator passwords are not changed by bootstrap settings. This repository has no default administrator password.

## Email

Email delivery is disabled unless `Email:Enabled` is explicitly enabled. Registration supports signing in without email confirmation. Password recovery and email-change workflows are unavailable when email is disabled. Configure a supported email service before enabling delivery.

## Deploy

The application is prepared for framework-dependent deployment to a Windows host with .NET 9 and SQL Server, including MonsterASP.

Copy `appsettings.Production.example.json` to `appsettings.Production.json` and fill the private connection string and bootstrap settings. The example file is excluded from publishing. Keep the completed production configuration, publish profiles and passwords outside Git.

Enable HTTPS on the host, use the Production environment, then publish in Release mode. Local and hosted databases are independent. Back up data separately from the source repository.

## Assets and private data

Group photos and branding are part of the app's static assets. The payment QR image is inherited from the original application; verify or replace its destination before accepting deposits.

Member records, uploaded payment proofs, database backups, production settings and deployment credentials are not included in this repository.
