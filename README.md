# AcxiomCRM — Sales Operations Portal

AcxiomCRM is a small role-aware CRM application built from the supplied project specification. The implementation is intentionally organized as a conventional ASP.NET Core MVC application so that each CRM responsibility is easy to inspect and explain during a project review.

## Technology choices

- C# with ASP.NET Core MVC on .NET 8
- ASP.NET Core Identity for accounts, password hashing, cookies and lockout
- Entity Framework Core with SQLite for persistence
- Razor Views for the server-rendered interface
- Bootstrap 5 for responsive layout
- Chart.js for dashboard visualisation
- REST controllers for customer, lead, opportunity and reporting data

## Functional areas

Authentication and role control, dashboard KPIs, customers, leads and conversion, opportunities, follow-ups, activities, user administration, audit history and reports are included. The supplied specification calls for the three roles Admin, Manager and SalesExecutive; the application enforces those roles on the server.

## Validation and security

The project uses Data Annotation validation in the models and additional server-side business checks. Examples include unique customer email/phone, valid lead states, positive opportunity amounts, probability between 0 and 100, future dates for active opportunities and valid dates for new follow-ups.

ASP.NET Core Identity handles password hashing and account lockout. State-changing MVC requests use anti-forgery validation. Protected controllers and API endpoints require authentication and role checks where appropriate. Audit entries capture important security and business operations without recording passwords.

## Run locally

1. Install the .NET 8 SDK.
2. Open the `AcxiomCRM` folder in VS Code.
3. Run `dotnet restore`.
4. Run `dotnet run`.
5. Browse to the HTTPS address printed by the terminal.

The SQLite file `acxiomcrm_data.db` is created automatically on first start.

## Development accounts

- Admin — `admin@acxiomcrm.local` / `Admin@123`
- Manager — `manager@acxiomcrm.local` / `Manager@123`
- Sales Executive — `sales@acxiomcrm.local` / `Sales@123`

These are local demonstration credentials only and should be changed before deployment.

## API surface

- `GET/POST /api/customers`
- `GET/PUT/DELETE /api/customers/{id}`
- `GET/POST /api/leads`
- `GET/POST /api/opportunities`
- `GET /api/reports/pipeline`

The API responses are DTO-shaped rather than returning Identity credentials or other sensitive fields.

## Project structure

`Controllers` contains web and API endpoints, `Models` contains domain data, `ViewModels` contains screen-specific data, `Services` contains shared application behavior, `Data` contains the EF Core context and seed process, and `Views/wwwroot` contain the presentation layer.
