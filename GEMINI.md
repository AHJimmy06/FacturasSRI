# GEMINI Project Context: FacturasSRI

## Project Overview

This is a .NET 8 web application named "FacturasSRI" for managing electronic invoices, likely for compliance with the SRI (Servicio de Rentas Internas), the Ecuadorian tax authority. The application is built using a **Blazor Server** frontend and a backend API.

It follows a **Clean Architecture** pattern, separating concerns into four main projects:
-   `FacturasSRI.Domain`: Contains the core business entities and enums.
-   `FacturasSRI.Application`: Defines business logic interfaces and Data Transfer Objects (DTOs).
-   `FacturasSRI.Infrastructure`: Implements data access and external services. It uses **Entity Framework Core** with a **PostgreSQL** database (`Npgsql`).
-   `FacturasSRI.Web`: The presentation layer, containing Blazor components, controllers for the API, and the application's entry point.

Key features include management of Invoices (`Facturas`), Customers (`Clientes`), Products (`Productos`), Purchases (`Compras`), and Inventory Adjustments. Authentication is handled via JWT and Cookies.

The file `instruccion.txt` contains a detailed set of instructions for a future task involving adding a 'Created By' column to several pages, which indicates a focus on improving auditability.

## Building and Running

### Prerequisites
- .NET 8 SDK (version 8.0.415 or later is specified in `global.json`)
- PostgreSQL database

### Configuration
1.  The database connection string needs to be configured in `FacturasSRI.Web/appsettings.json`. The current configuration looks for a connection string named `"DefaultConnection"`.
2.  The JWT settings (`Issuer`, `Audience`) in the same file also need to be replaced with actual values.

### Commands
-   **Build the solution:**
    ```bash
    dotnet build FacturasSRI.sln
    ```
-   **Run the application:** The startup project is `FacturasSRI.Web`.
    ```bash
    dotnet run --project FacturasSRI.Web
    ```
-   **Database Migrations:** To apply Entity Framework migrations.
    ```bash
    dotnet ef database update --project FacturasSRI.Infrastructure --startup-project FacturasSRI.Web
    ```

### Testing
-   TODO: No test projects were found in the solution. Tests may need to be created to ensure code quality and prevent regressions.

## Development Conventions

-   **Architecture**: Strictly follow the existing Clean Architecture pattern.
    -   Domain logic goes into `FacturasSRI.Domain`.
    -   Application logic (services, DTOs) goes into `FacturasSRI.Application`.
    -   Implementation details (database, external APIs) go into `FacturasSRI.Infrastructure`.
    -   UI and API controllers go into `FacturasSRI.Web`.
-   **Data Access**: Use Entity Framework Core and the repository/service pattern established in the `Infrastructure` project. Queries should be performed in the service implementations (e.g., `InvoiceService.cs`).
-   **Frontend**: The UI is built with Blazor Server components. UI logic should be contained within the `.razor` files.
-   **API**: Backend APIs are built with ASP.NET Core controllers.
-   **Coding Style**: Follow the existing C# coding style, including naming conventions and asynchronous programming (`async`/`await`).
-   **Localization**: The application is localized for `es-EC` (Spanish - Ecuador), with resource files in `FacturasSRI.Web/Resources`.
