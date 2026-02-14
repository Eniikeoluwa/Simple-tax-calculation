# Nova API

A comprehensive payment and financial management API built with ASP.NET Core, designed to handle vendor management, payment processing, and scheduling operations for multi-tenant organizations.

## Overview

Nova API is a multi-tenant financial management system that provides secure, scalable APIs for managing vendors, banks, payments, and scheduled payment operations. Built with modern .NET architecture using CQRS pattern with MediatR, the system ensures clean separation of concerns and maintainable code.

## Key Features

### Authentication & Authorization
- JWT-based authentication with access and refresh tokens
- Secure user registration and login
- Password reset functionality
- Multi-tenant user isolation

### Vendor Management
- Create and manage vendor profiles
- Link vendors to bank accounts
- Track tax information (TIN, VAT, WHT)
- Multi-tenant vendor isolation

### Bank Management
- Manage bank information and details
- Bank account linking for vendors
- Support for multiple banks per tenant

### Payment Processing
- Payment creation and tracking
- Multiple payment methods support
- Payment status management
- Tenant-specific payment records

### Scheduled Operations
- Bulk payment scheduling
- GAPS (Government Automated Payment System) scheduling
- Automated payment processing
- Schedule management and tracking

### Tenant Management
- Multi-tenant architecture
- Tenant-specific data isolation
- User-tenant associations
- Tenant activation/deactivation

## Tech Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **Validation**: FluentValidation
- **Pattern**: CQRS with MediatR
- **Password Hashing**: BCrypt.NET
- **Architecture**: Clean Architecture with Domain, Infrastructure, API, and Contracts layers

## Project Structure

```
src/
├── Nova.API/              # Web API layer (Controllers, Middleware)
│   ├── Controllers/       # API endpoints
│   ├── Application/       # Business logic (Commands, Queries, Services)
│   └── Extensions/        # Extension methods
├── Nova.Contracts/        # DTOs and Request/Response models
├── Nova.Domain/           # Domain entities and business rules
└── Nova.Infrastructure/   # Data access and external services
```

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL database
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Setup

1. Clone the repository
   ```bash
   git clone https://github.com/Eniikeoluwa/Novastart.git
   cd Novastart
   ```

2. Configure environment variables (see `.env.example`)
   ```bash
   cp .env.example .env.local
   # Edit .env.local with your database and JWT settings
   ```

3. Run database migrations
   ```bash
   cd src/Nova.API
   dotnet ef database update
   ```

4. Run the application
   ```bash
   dotnet run
   ```

5. Access the API at `http://localhost:8080`

## API Endpoints

### Authentication
- `POST /api/auth/signup` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password

### Vendors
- `GET /api/vendors` - Get all vendors
- `GET /api/vendors/{id}` - Get vendor by ID
- `POST /api/vendors` - Create vendor
- `PUT /api/vendors/{id}` - Update vendor
- `DELETE /api/vendors/{id}` - Delete vendor

### Banks
- `GET /api/banks` - Get all banks
- `POST /api/banks` - Create bank
- `PUT /api/banks/{id}` - Update bank

### Payments
- `GET /api/payments` - Get all payments
- `GET /api/payments/{id}` - Get payment by ID
- `POST /api/payments` - Create payment
- `PUT /api/payments/{id}` - Update payment

### Tenants
- `GET /api/tenants` - Get all tenants
- `POST /api/tenants` - Create tenant

## Security

- JWT tokens with configurable expiration
- Secure password hashing with BCrypt
- Multi-tenant data isolation
- Environment variable configuration for sensitive data
- CORS policy for controlled access

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.