# Hexagonal Architecture Template (.NET Core)

This project demonstrates the implementation of Hexagonal Architecture (Ports and Adapters) in .NET Core, providing a clean and maintainable structure for building enterprise applications.

## Project Structure

The solution is organized into the following projects:

### Domain Layer (`HexagonalArchitecture.Domain`)
- Contains business entities and logic
- Defines domain interfaces (ports)
- Implements domain services
- Contains business rules and validation

### Application Layer (`HexagonalArchitecture.Application`)
- Implements use cases
- Contains application services
- Defines DTOs (Data Transfer Objects)
- Handles business workflows

### Infrastructure Layer (`HexagonalArchitecture.Infrastructure`)
- Implements adapters for external services
- Contains database implementations
- Manages external dependencies
- Implements repositories

### API Layer (`HexagonalArchitecture.Api`)
- Handles HTTP requests
- Manages API endpoints
- Implements controllers
- Handles authentication/authorization

## Key Features

- **Clean Architecture**: Follows Hexagonal Architecture principles
- **Dependency Injection**: Uses built-in .NET Core DI container
- **Domain-Driven Design**: Implements DDD patterns and practices
- **CQRS Pattern**: Separates read and write operations
- **Repository Pattern**: Abstracts data access
- **Unit of Work**: Manages database transactions
- **Validation**: Uses FluentValidation
- **Logging**: Implements structured logging with Serilog
- **API Documentation**: Uses Swagger/OpenAPI
- **Authentication**: JWT-based authentication with Keycloak
- **Multi-tenancy**: Supports multi-tenant data isolation
- **Testing**: Supports unit and integration tests

## Getting Started

1. Clone the repository
2. Restore NuGet packages
3. Update database connection string in `appsettings.json`
4. Run database migrations
5. Start Keycloak container:
   ```bash
   docker run -p 8080:8080 -e KEYCLOAK_ADMIN=admin -e KEYCLOAK_ADMIN_PASSWORD=admin quay.io/keycloak/keycloak:latest start-dev
   ```
6. Configure Keycloak:
   ```powershell
   ./scripts/setup-keycloak.ps1
   ```
7. Start the application

## Technologies

- .NET Core 8.0
- Entity Framework Core
- MediatR
- FluentValidation
- Serilog
- Swagger/OpenAPI
- xUnit
- Keycloak
- JWT Authentication

## Architecture Overview

This project follows the Hexagonal Architecture pattern, also known as Ports and Adapters. The key principles include:

- **Domain-Centric**: Business logic is at the core
- **Dependency Rule**: Dependencies point inward
- **Isolation**: Business logic is isolated from external concerns
- **Testability**: Easy to test due to clear separation of concerns

## Multi-tenancy Support

The application implements a multi-tenant architecture where each tenant's data is isolated from others:

- **Tenant Identification**: Each tenant is identified by their realm ID in Keycloak
- **Data Isolation**: Repository layer automatically filters data based on tenant ID
- **Security**: Unauthorized access to other tenant's data is prevented
- **Scalability**: Each tenant can have their own database (planned for future)

### How Multi-tenancy Works

1. **Authentication**: Users authenticate through Keycloak
2. **Tenant ID**: The realm ID from Keycloak is used as the tenant ID
3. **Data Access**: All database queries are automatically filtered by tenant ID
4. **Data Creation**: New entities are automatically assigned the current tenant ID
5. **Security**: Attempts to access data from other tenants result in 401 Unauthorized

### Future Plans

- Dynamic database creation per tenant
- Tenant-specific configurations
- Resource isolation between tenants
- Tenant management interface

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request
