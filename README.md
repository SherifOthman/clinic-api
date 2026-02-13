# Clinic Management API

A comprehensive multi-tenant clinic management system built with .NET 10, demonstrating enterprise-grade architecture and modern development practices.

## 🌐 Live Demo

- **API Documentation**: http://clinic-api.runasp.net/scalar/v1
- **Dashboard**: https://clinic-dashboard-ecru.vercel.app
- **Website**: https://clinic-website-lime.vercel.app

## 📦 Repositories

- **Backend API**: https://github.com/SherifOthman/clinic-api
- **Dashboard**: https://github.com/SherifOthman/clinic-dashboard
- **Website**: https://github.com/SherifOthman/clinic-website

## Key Technical Achievements

### Architecture & Design

- **Vertical Slice Architecture** - Features organized as independent, self-contained slices for better maintainability and team scalability
- **Multi-Tenancy** - Automatic data isolation using EF Core global query filters, ensuring complete separation between clinics
- **Soft Delete Pattern** - Complete audit trail with CreatedBy, UpdatedAt, DeletedBy for compliance and data recovery

### Security & Authentication

- JWT-based authentication with refresh tokens in HTTP-only cookies
- Role-based authorization with multiple user types
- Email verification workflow
- Password reset with secure token validation
- Multi-tenant data isolation at the database level

### API Design & Documentation

- RESTful API following HTTP semantics and best practices
- RFC 7807 Problem Details for standardized error responses
- Interactive API documentation with Scalar/OpenAPI
- Internationalization support with 40+ error codes for Arabic/English

### Business Features

- **Patient Management** - Complete profiles, medical history, chronic diseases, multiple contacts
- **Appointment System** - Scheduling with state machine (Pending → Confirmed → Completed/Cancelled)
- **Billing & Payments** - Flexible invoicing with discounts, taxes, partial payments, and overdue tracking
- **Inventory Management** - Medicine stock tracking with expiry monitoring and low stock alerts
- **Location Services** - Bilingual location data (Arabic/English) with caching strategy

## Technology Stack

- **.NET 10** - Latest framework with minimal APIs
- **Entity Framework Core 10** - ORM with SQL Server
- **ASP.NET Core Identity** - User management
- **JWT + Refresh Tokens** - Secure authentication
- **Serilog** - Structured logging
- **Scalar/Swagger** - API documentation
- **MailKit** - Email notifications
- **GeoNames API** - Location data integration

## What This Demonstrates

- Modern .NET development with latest framework features
- Clean architecture principles and separation of concerns
- Database design with proper relationships and constraints
- Security best practices and authentication patterns
- API design and documentation standards
- Multi-tenancy implementation
- Error handling and logging strategies
- Integration with external services
- Production deployment and database migrations

## License

MIT
