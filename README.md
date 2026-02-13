# Clinic Management API

A comprehensive clinic management system built with .NET 10, featuring **Vertical Slice Architecture** and **Multi-Tenancy**.

## 🌐 Live Demo

- **API Documentation**: http://clinic-api.runasp.net/scalar/v1
- **Frontend Dashboard**: https://clinic-dashboard-ecru.vercel.app

## Architecture & Design Patterns

### Vertical Slice Architecture

Features are organized as independent, self-contained slices rather than traditional layered architecture. Each feature owns its complete vertical stack from endpoint to database, promoting high cohesion and low coupling.

### Multi-Tenancy

Automatic clinic-based data isolation using EF Core global query filters. All queries are automatically filtered by ClinicId from JWT claims, ensuring complete data isolation between clinics.

### Domain-Driven Design

- Rich domain models with business logic encapsulated in entities
- Soft delete pattern with full audit trail (CreatedBy, UpdatedAt, DeletedBy)
- Aggregate roots managing entity relationships
- Value objects for complex types

### API Design

- RESTful endpoints following HTTP semantics
- RFC 7807 Problem Details for standardized error responses
- Scalar API documentation with OpenAPI/Swagger
- Versioned API with backward compatibility considerations

## Core Features

### Authentication & Authorization

- JWT access tokens (60min) + refresh tokens (30 days) in HTTP-only cookies
- Email confirmation required before login
- Password reset via email with token validation
- Role-based authorization (SuperAdmin, ClinicOwner, Doctor, Receptionist)
- Multi-tenancy with automatic clinic-based data isolation

### Patient Management

- Complete patient profiles with demographics and medical history
- Multiple phone numbers per patient with primary designation
- Chronic disease associations and allergy tracking
- Pagination, search, and filtering capabilities
- Soft delete with audit trail

### Appointment System

- Appointment scheduling with queue management
- State machine: Pending → Confirmed → Completed/Cancelled
- Doctor working day scheduling
- Appointment type configuration with pricing per branch

### Billing & Payments

- Flexible invoices with medicines, services, and supplies
- Discount and tax support with automatic calculations
- Multiple payment methods with partial payment tracking
- Invoice status workflow: Draft → Issued → Paid/Cancelled
- Overdue tracking and payment history

### Inventory Management

- Medicine stock tracking with box/strip system
- Expiry date monitoring and discontinued flag
- Medical services and supplies catalog
- Stock movement tracking (add/remove with reasons)
- Low stock alerts

### Location Services

- Bilingual location data (Arabic/English) via GeoNames API
- Countries, states, and cities with 24-hour caching
- GeoName ID-based location references

### Error Handling & Internationalization

- RFC 7807 Problem Details with i18n error codes
- 40+ predefined error codes for frontend translation
- Structured error responses with trace IDs for debugging
- Support for Arabic and English error messages

## Technology Stack

- **.NET 10.0** - Latest framework with minimal APIs
- **EF Core 10.0** - ORM with SQL Server, 36 entity configurations
- **ASP.NET Core Identity** - User management and authentication
- **JWT + Refresh Tokens** - Secure authentication with HTTP-only cookies
- **Scalar.AspNetCore** - Modern API documentation
- **Serilog** - Structured logging with file and console sinks
- **libphonenumber** - International phone number validation
- **MailKit** - Email service for notifications
- **GeoNames API** - Location data integration

## Project Statistics

- **15 Feature Areas** - Organized by business capability
- **53 RESTful Endpoints** - Complete CRUD operations
- **44 Domain Entities** - Rich domain models with business logic
- **18 Infrastructure Services** - Cross-cutting concerns
- **15 Domain Enums** - Type-safe enumerations
- **2 Custom Middleware** - Global exception handling, JWT cookie processing

## License

MIT
