# DeliveryDash Backend API

A robust . NET-based backend API for the DeliveryDash application - a multi-vendor delivery and ordering platform designed for residential complexes.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Database](#database)
- [Real-time Notifications](#real-time-notifications)
- [Configuration](#configuration)

## 🎯 Overview

DeliveryDash is a comprehensive delivery platform that connects vendors with tenants in residential buildings. The system supports:

- **Multi-vendor marketplace** - Multiple vendors (restaurants, grocery stores, etc.) can register and sell products
- **Order management** - Complete order lifecycle from creation to delivery
- **Building management** - Hierarchical structure of buildings, floors, and apartments
- **Real-time notifications** - Push notifications via SignalR
- **User management** - Role-based access control (SuperAdmin, Admin, Vendor, Tenant)

## 🏗️ Architecture

The application follows **Clean Architecture** principles with four main layers:

```
DeliveryDash. sln
├── DeliveryDash.API            # Presentation Layer (Controllers, Hubs, Handlers)
├── DeliveryDash.Application    # Application Layer (Services, DTOs, Validators)
├── DeliveryDash.Domain         # Domain Layer (Entities, Enums, Exceptions)
└── DeliveryDash.Infrastructure # Infrastructure Layer (Data Access, Repositories)
```

### Layer Responsibilities

| Layer | Description |
|-------|-------------|
| **DeliveryDash.API** | REST API controllers, SignalR hubs, exception handlers, and middleware configuration |
| **DeliveryDash.Application** | Business logic services, request/response DTOs, FluentValidation validators, and service interfaces |
| **DeliveryDash.Domain** | Core entities, domain enums, constants, and custom exceptions |
| **DeliveryDash. Infrastructure** | Entity Framework Core DbContext, repository implementations, and external service integrations |

## 🛠️ Technologies

- **.NET 9** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **Redis** - Caching
- **SignalR** - Real-time communication
- **FluentValidation** - Request validation
- **ASP.NET Core Identity** - Authentication & Authorization
- **JWT Bearer Tokens** - API authentication
- **Scalar** - API documentation (OpenAPI)
- **AspNetCoreRateLimit** - Rate limiting

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9. 0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download) (optional, for caching)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Renua-Hassan/DeliveryDash-BackEnd.git
   cd DeliveryDash-BackEnd
   ```

2. **Configure the database connection**
   
   Update `appsettings. json` with your PostgreSQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DbConnectionString": "Host=localhost;Database=DeliveryDash;Username=your_user;Password=your_password"
     }
   }
   ```

3. **Configure JWT settings**
   ```json
   {
     "JwtOptions": {
       "Key": "your-secret-key-here",
       "Issuer": "DeliveryDash",
       "Audience": "DeliveryDash"
     }
   }
   ```

4. **Configure Redis (optional)**
   ```json
   {
     "Redis": {
       "ConnectionString": "localhost:6379",
       "DefaultDatabaseId": 0,
       "InstanceName": "DeliveryDash:"
     }
   }
   ```

5. **Run the application**
   ```bash
   cd DeliveryDash. API
   dotnet run
   ```

6. **Access the API documentation**
   
   Navigate to `/scalar/v1` for the interactive API documentation. 

## 📡 API Endpoints

### Account Controller (`/DeliveryDashApi/Account`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Register a new user | ❌ |
| POST | `/login` | User login | ❌ |
| POST | `/logout` | User logout | ✅ |
| GET | `/me` | Get current user info | ✅ |
| POST | `/refresh` | Refresh access token | ❌ |
| POST | `/validate-token` | Validate JWT token | ❌ |

### Vendor Controller (`/DeliveryDashApi/Vendor`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get vendor by ID | ✅ Admin |
| GET | `/user/{userId}` | Get vendor by user ID | ✅ Admin |
| GET | `/` | Get all vendors (paginated) | ✅ Admin |
| POST | `/` | Create a new vendor | ✅ Admin |
| PUT | `/{id}` | Update vendor | ✅ Admin |
| DELETE | `/{id}` | Delete vendor | ✅ Admin |

### Product Controller (`/DeliveryDashApi/Product`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get product by ID | ❌ |
| GET | `/` | Get all products (paginated, filterable) | ❌ |
| GET | `/vendor/{vendorId}` | Get products by vendor | ❌ |
| GET | `/category/{categoryId}` | Get products by category | ❌ |
| GET | `/my-products` | Get current vendor's products | ✅ Vendor |
| POST | `/` | Create a product | ✅ Vendor |
| PUT | `/{id}` | Update a product | ✅ Vendor |
| DELETE | `/{id}` | Delete a product | ✅ Vendor |

### Order Controller (`/DeliveryDashApi/Order`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/` | Create an order | ✅ Tenant |
| GET | `/{id}` | Get order by ID | ✅ |
| GET | `/number/{orderNumber}` | Get order by order number | ✅ |
| GET | `/` | Get orders (paginated) | ✅ |
| PUT | `/{id}/status` | Update order status | ✅ Admin/Vendor |
| POST | `/{id}/cancel` | Cancel an order | ✅ |

### Category Controller (`/DeliveryDashApi/Category`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get category by ID | ❌ |
| GET | `/` | Get all categories | ❌ |
| GET | `/top-level` | Get top-level categories | ❌ |
| GET | `/subcategories/{parentId}` | Get subcategories | ❌ |
| GET | `/paged` | Get categories (paginated) | ❌ |
| POST | `/` | Create category | ✅ Admin |
| PUT | `/{id}` | Update category | ✅ Admin |
| DELETE | `/{id}` | Delete category | ✅ Admin |

### Building Controller (`/DeliveryDashApi/Building`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/{id}` | Get building by ID | ✅ Admin |
| GET | `/` | Get all buildings | ✅ Admin |
| POST | `/` | Create building | ✅ Admin |
| PUT | `/BuildingName/{id}` | Update building | ✅ Admin |
| DELETE | `/{id}` | Delete building | ✅ Admin |
| POST | `/{id}/floor` | Add floor to building | ✅ Admin |
| DELETE | `/floor/{floorId}` | Delete floor | ✅ Admin |
| POST | `/floor/{floorId}/apartment` | Add apartment | ✅ Admin |
| PUT | `/apartment/{apartmentId}` | Update apartment | ✅ Admin |

### Notification Controller (`/DeliveryDashApi/Notification`)

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get user notifications | ✅ |
| GET | `/unread-count` | Get unread count | ✅ |
| PUT | `/{id}/read` | Mark as read | ✅ |
| PUT | `/mark-all-read` | Mark all as read | ✅ |
| DELETE | `/{id}` | Delete notification | ✅ |

## 🔐 Authentication

The API uses **JWT Bearer Token** authentication with the following features:

- Access tokens for API authentication
- Refresh tokens for session management
- Role-based authorization (SuperAdmin, Admin, Vendor, Tenant)
- Secure password requirements (min 8 chars, uppercase, lowercase, digit)

### Roles & Permissions

| Role | Description |
|------|-------------|
| **SuperAdmin** | Full system access |
| **Admin** | Manage vendors, buildings, categories, users |
| **Vendor** | Manage own products and orders |
| **Tenant** | Place orders, manage profile |

## 💾 Database

### Entity Model

```
User (ASP.NET Identity)
├── Address
├── Vendor (1:1 for vendor users)
│   ├── Products
│   └── Orders
├── Orders (for tenants)
└── Notifications

Building
├── Floors
│   └── Apartments
│       └── Address

Product
├── Category (hierarchical)
└── Vendor

Order
├── OrderItems
├── User (tenant)
└── Vendor
```

### Key Entities

- **User** - Extended IdentityUser with profile information
- **Vendor** - Store/restaurant with products
- **Product** - Items available for purchase
- **Category** - Hierarchical product categories
- **Order/OrderItem** - Purchase records
- **Building/Floor/Apartment** - Residential structure
- **Address** - User delivery addresses
- **Notification** - User notifications

## 📢 Real-time Notifications

The application uses **SignalR** for real-time push notifications. 

### Hub Endpoint
```
/hubs/notifications
```

### Features
- Order status updates
- New order notifications for vendors
- System announcements

## ⚙️ Configuration

### Environment-specific Settings

The application supports different configurations for Development and Production environments:

- **Development**: Database migrations and seeding run automatically
- **Production**: Manual migration management recommended

### Rate Limiting

IP-based rate limiting is configured to prevent API abuse. 

### Static Files

Product and user images are served from the `/Uploads` directory with caching enabled.

## 📁 Project Structure

```
DeliveryDash. API/
├── Controllers/           # API endpoints
├── Handlers/              # Exception handlers
├── Hubs/                  # SignalR hubs
├── Services/              # API-layer services
└── Program.cs             # Application entry point

DeliveryDash. Application/
├── Abstracts/             # Interfaces
│   ├── IRepository/       # Repository contracts
│   └── IService/          # Service contracts
├── Extensions/            # Extension methods
├── Options/               # Configuration options
├── Requests/              # Request DTOs
├── Responses/             # Response DTOs
├── Services/              # Business logic
└── Validators/            # FluentValidation validators

DeliveryDash. Domain/
├── Constants/             # Domain constants
├── Entities/              # Domain models
├── Enums/                 # Enumerations
└── Exceptions/            # Custom exceptions

DeliveryDash. Infrastructure/
├── Data/                  # DbContext & migrations
├── Options/               # Infrastructure options
├── Processors/            # Token processors
├── Repositories/          # Data access
└── Services/              # External services
```

## 🔗 Health Check

The API exposes a health check endpoint at:
```
GET /health
```

## 📄 License

This project is private and proprietary. 

## 👤 Author

**Renua Hassan** - [GitHub Profile](https://github. com/Renua-Hassan)
