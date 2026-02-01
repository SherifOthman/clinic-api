# 🏥 ClinicCare API - Multi-Tenant Healthcare Backend

> **.NET 10 API** with Clean Architecture, CQRS, and multi-clinic support

⚠️ **This application is currently in development and not yet production-ready**

📝 **This README was written with AI assistance**

## 🎯 **What This Project Is**

A healthcare management API that supports multiple clinics with role-based access and patient management. Built to demonstrate modern .NET development practices and architectural patterns.

### **🚀 Live Demo**

- **API Documentation**: http://clinic-api.runasp.net/swagger

## 🛠️ **Technology Stack**

| Technology                   | Purpose                                 |
| ---------------------------- | --------------------------------------- |
| **.NET 10**                  | Latest web API framework                |
| **Entity Framework Core 10** | Database ORM with Code-First migrations |
| **SQL Server**               | Database with LocalDB for development   |
| **MediatR**                  | CQRS pattern implementation             |
| **JWT + Refresh Tokens**     | Stateless authentication                |
| **Mapster**                  | Object-to-object mapping                |
| **FluentValidation**         | Input validation                        |
| **Serilog**                  | Structured logging                      |
| **Swagger/OpenAPI**          | API documentation                       |

## ✅ **Implemented Features**

### **Multi-Tenant Architecture**

- ✅ Users can own/manage multiple clinics
- ✅ Clinic switching with JWT context updates
- ✅ Role-based access per clinic (ClinicOwner, Admin, Doctor, Receptionist)
- ✅ Data isolation between clinics using query filters
- ✅ Subscription-based clinic limits

### **Authentication & Security**

- ✅ User registration with email verification
- ✅ JWT access tokens (60 min) with refresh token rotation (30 days)
- ✅ Password reset with email tokens
- ✅ Password hashing with ASP.NET Core Identity
- ✅ Role-based authorization middleware
- ✅ Rate limiting protection (IP + User based)
- ✅ Global exception handling
- ✅ CORS configuration

### **Patient Management**

- ✅ Complete patient profiles (name, DOB, gender, address)
- ✅ Multiple phone numbers per patient
- ✅ Chronic disease tracking and assignment
- ✅ GeoNames integration for location data
- ✅ Advanced search and filtering with pagination
- ✅ Soft delete functionality

### **User Management**

- ✅ User profiles with specializations
- ✅ Multi-clinic user relationships with roles
- ✅ Profile image upload/management
- ✅ Email confirmation system

### **Subscription System**

- ✅ Three-tier subscription plans (Solo, Growing, Network)
- ✅ Feature-based access control flags
- ✅ Clinic count limitations per plan
- ✅ Branch limitations per plan
- ✅ Advanced reporting and API access flags

### **Location Services**

- ✅ GeoNames API integration (working with credentials)
- ✅ Country/state/city lookup with caching
- ✅ Location search functionality
- ✅ Phone number validation by country
- ✅ REST Countries API integration

### **File Management**

- ✅ Local file storage service
- ✅ Profile image upload/download/delete
- ✅ File validation and security

### **Background Services**

- ✅ Expired refresh token cleanup (every 6 hours)
- ✅ Rate limit entries cleanup (every 1 hour)
- ✅ Database initialization with seed data

### **Email System**

- ✅ SMTP email sending with MailKit
- ✅ Gmail SMTP integration (configured)
- ✅ Email confirmation templates
- ✅ Password reset email templates

## 🏗️ **Clean Architecture**

**4-Layer Structure:**

- **API Layer** (10 controllers) - HTTP endpoints, middleware
- **Application Layer** (40+ handlers) - Business logic, CQRS
- **Domain Layer** (13 entities) - Business rules, entities
- **Infrastructure Layer** - Database, external services

**Key Patterns:**

- **CQRS** with MediatR for command/query separation
- **Repository Pattern** for data access abstraction
- **Unit of Work** for transaction management
- **Dependency Injection** throughout all layers

## 📊 **Database Schema**

**Core Entities (13 tables):**

- **User** - ASP.NET Identity with multi-clinic support
- **Clinic** - Healthcare facilities with subscription plans
- **ClinicBranch** - Multiple locations per clinic with GeoNames
- **UserClinic** - Many-to-many with roles and ownership flags
- **Patient** - Patient records with soft delete
- **PatientPhoneNumber** - Multiple phone numbers support
- **ChronicDisease** - Medical conditions reference data
- **PatientChronicDisease** - Patient-disease relationships
- **SubscriptionPlan** - Pricing tiers with feature flags
- **Specialization** - Medical specializations
- **RefreshToken** - Token management with revocation
- **RateLimitEntry** - API protection

## � **API Endpoints (50+ endpoints)**

**Authentication (13 endpoints)**

- Register, Login, Logout, ConfirmEmail, ForgotPassword, ResetPassword
- ResendEmailVerification, ChangePassword, UpdateProfile, UpdateProfileImage
- GetMe, GetUserClinics, SwitchClinic

**Patient Management (6 endpoints)**

- GetAll, GetPaginated, GetById, Create, Update, Delete

**Chronic Diseases (6 endpoints)**

- GetAll, GetPaginated, GetById, Create, Update, Delete

**Location Services (8 endpoints)**

- GetCountries, GetStates, GetCities, SearchCities, ValidatePhone
- GeoNamesHealth, GetCountryPhoneCodes, GetLocationHierarchy

**Subscription Plans (5 endpoints)**

- GetAll, GetById, Create, Update, Delete

**Admin Functions (8 endpoints)**

- User management, Clinic management, User status control

**File Management (2 endpoints)**

- GetFile, DeleteFile

**Onboarding (2 endpoints)**

- GetSubscriptionPlans, CompleteOnboarding

**Analytics (1 endpoint)**

- GetDashboardData (returns mock data)

## 🚀 **Quick Start**

```bash
# Prerequisites: .NET 10, SQL Server LocalDB
git clone <repo>
cd "Clinic API"
dotnet restore
dotnet ef database update --project src/ClinicManagement.Infrastructure
dotnet run --project src/ClinicManagement.API
# Access: http://localhost:5000/swagger
```

## 🎯 **Subscription Plans**

**Solo Practice** - $29.99/month

- 1 clinic, 1 branch, 100 patients, basic reporting

**Growing Clinic** - $79.99/month

- 3 clinics, 10 branches, 1000 patients, advanced reporting

**Healthcare Network** - $199.99/month

- Unlimited clinics/branches, unlimited patients, API access, priority support

## 📊 **Project Stats**

- **Controllers**: 10
- **Entities**: 13
- **API Endpoints**: 50+
- **CQRS Handlers**: 40+
- **Background Services**: 2
- **External Integrations**: 3 (GeoNames, Gmail SMTP, REST Countries)

---

**Demonstrates:** Modern .NET development, Clean Architecture, Multi-tenancy, CQRS, Security implementation
