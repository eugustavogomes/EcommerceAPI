# ECommerceAPI

A RESTful API for an e-commerce platform built with ASP.NET Core 8 and PostgreSQL.

## Tech Stack

- **Runtime:** .NET 8
- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core 8
- **Authentication:** JWT Bearer
- **Password hashing:** BCrypt
- **Container:** Docker / Docker Compose

## Project Structure

```
ECommerceAPI.sln
├── ECommerceAPI.API          # Controllers, Program.cs, entry point
├── ECommerceAPI.Application  # Business logic and services
├── ECommerceAPI.Domain       # Entities and DTOs
└── ECommerceAPI.Infrastructure # DbContext, EF migrations, DI setup
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

### 1. Start the database

```bash
docker compose up -d
```

### 2. Apply migrations

```bash
dotnet ef database update \
  --project ECommerceAPI.Infrastructure \
  --startup-project ECommerceAPI.API
```

### 3. Run the API

```bash
dotnet run --project ECommerceAPI.API
```

The API will be available at `https://localhost:5001`.

## Endpoints

### Auth

| Method | Route | Description | Auth required |
|--------|-------|-------------|---------------|
| POST | `/api/auth/register` | Create a new user account | No |
| POST | `/api/auth/login` | Authenticate and receive a JWT | No |

#### Register

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "yourpassword"
}
```

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "yourpassword"
}
```

Response:

```json
{
  "token": "<jwt>"
}
```

## Configuration

App settings are in `ECommerceAPI.API/appsettings.json`. For local development, override sensitive values in `appsettings.Development.json` (not committed).

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:Key` | Secret key for signing tokens (min. 32 chars) |
| `Jwt:Issuer` | JWT issuer |
| `Jwt:Audience` | JWT audience |
