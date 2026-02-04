# 🏥 ClinicCare API - Multi-Tenant Healthcare Management System

> **.NET 10 API** with Clean Architecture, CQRS, and comprehensive clinic management

⚠️ **This application is currently in development and not yet production-ready**

📝 **This README was written with AI assistance**

## 🎯 **What This Project Is**

A comprehensive healthcare management API that supports multi-clinic operations with advanced features including appointment scheduling, patient management, inventory control, billing, andt practices and Clean Architecture principles.

### **🚀 Live Demo**

- **API Documentation**: https://clinic-api.runasp.net/swagger

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

### **Authentication & Security**

- ✅ User registration with email verification
- ✅ JWT access tokens with refresh token rotation
- ✅ Password reset with email tokens
- ✅ Password hashing with ASP.NET Core Identity
- ✅ Role-based authorization (SuperAdmin, ClinicOwner, Doctor, Nurse, Receptionist, Patient)
- ✅ Global exception handling with structured error responses
- ✅ CORS configuration

### **Clinic Management**

- ✅ Multi-clinic support with subscription-based limitations
- ✅ Clinic branch management with working hours
- ✅ Staff management with role assignments
- ✅ Doctor profiles with specializations
- ✅ Appointment type configuration with pricing

### **Patient Management**

- ✅ Comprehensive patient profiles with demographics
- ✅ Multiple phone numbers per patient
- ✅ Chronic disease tracking and management
- ✅ Patient-clinic relationships
- ✅ Medical file management

### **Appointment System**

- ✅ Appointment scheduling with queue management
  )
- ✅ Doctor-specific appointment management
- ✅ Appointment status tracking

### **Medical Records & Measurements**

- ✅ Vital signs tracking (blood pressure, heart rate, temperature, etc.)
- ✅ Specialization-specific measurement attributes
- ✅ Doctor-customizable measurement requirements
- ✅ Medical visit documentation

### **Inventory Management**

- ✅ Medicine inventory with stock tracking
- ✅ Medical supplies management
- ✅ Stock level monitoring and alerts
- ✅ Expiry date tracking

### **Billing & Financial**

- ✅ Invoice generation and management
- ✅ Payment processing and tracking
- ✅ Medical service pricing
- ✅ Multiple payment methods support

### **Subscription System**

- ✅ Two-tier subscription plans (Basic, Professional)
- ✅ Feature-based access control
- ✅ Usage limitations per plan
- ✅ Subscription management

### **Reference Data Management**

- ✅ Medical specializations (10 specializations with Arabic/English names)
- ✅ Chronic diseases (10 common conditions with descriptions)
- ✅ Measurement attributes (10 vital signs with data types)
- ✅ Appointment types (8 standard appointment categories)
- ✅ System roles and super admin user
- ✅ Automated reference data seeding

### **Onboarding System**

- ✅ Clinic setup wizard for new users
- ✅ Subscription plan selection
- ✅ Initial clinic and branch configuration
- ✅ Owner role assignment

## 🏗️ **Clean Architecture**

**4-Layer Structure:**

- **API Layer** (12 controllers) - HTTP endpoints, middleware, validation
  ogic, CQRS commands/queries
- **Domain Layer** (37 entities) - Business rules, domain models
- **Infrastructure Layer** - Database, external services, repositories

**Key Patterns:**

- **CQRS** with MediatR for command/query separation
- **Repository Pattern** for data access abstraction
- **Unit of Work** for transaction management
- **Dependency Injection** throughout all layers

## 📊 **Database Schema**

**Core Entities (37 tables):**

**Identity & Security:**
ionships

- **RefreshToken** - JWT token management

**Clinic Management:**

- **Clinic** - Healthcare facilities with subscription plans
- **ClinicBranch** - Multiple locations per clinic
- **Staff** - Clinic staff with role assignments
- **Doctor** - Doctor profiles with specializations

**Patient Management:**

- **Patient** - Patient master records
- **ClinicPatient** - Clinic-specific patient data
- **ClinicPatientPhone** - Multiple phone numbers
- **ClinicPatientChronicDisease** - Patient chronic conditions

**Appointments & Medical:**

- **Appointment** - Appointment scheduling
- **AppointmentType** - Appointment categories
- **MedicalVisit** - Visit documentation
- **MedicalFile** - Patient medical files

**Measurements:**

- **MeasurementAttribute** - Vital sign definitions
- **MedicalVisitMeasurement** - Visit measurements
- **DoctorMeasurementAttribute** - Doctor preferences
- **SpecializationMeasurementAttribute** - Specialization defaults

**Inventory:**

- **Medicine** - Pharmacy inventory
- **MedicalSupply** - Medical supplies stock
- **MedicalService** - Service catalog

**Billing:**

- **Invoice** - Billing documents
- **InvoiceItem** - Invoice line items
- **Payment** - Payment records

**Reference Data:**

- **ChronicDisease** - Medical conditions reference
- **Specialization** - Medical specializations
- **SubscriptionPlan** - Pricing tiers

## 🌱 **Database Seeding**

The application automatically seeds essential reference data on first run:

**System Data:**

- Super Admin user (superadmin@clini23!)
- 7 system roles (SuperAdmin, ClinicOwner, Doctor, Nurse, Receptionist, Patient)

**Medical Reference Data:**

- 10 medical specializations (Cardiology, Pediatrics, Internal Medicine, etc.)
- 10 vital sign measurements (Blood Pressure, Heart Rate, Temperature, etc.)
- 10 chronic diseases (Diabetes, Hypertension, Asthma, etc.)
- 8 appointment types (Initial Consultation, Follow-up, Emergency, etc.)

**Business Data:**

- 2 subscription plans (Basic $99/month, Professional $199/month)
- Specialization-measurement relationships

_Note: No sample user data is seeded - users create their own clinics and patients through the application._

## 📡 **API Endpoints (35+ endpoints)**

**Authentication (9 endpoints)**

- Register, Login, Logout, ConfirmEmail, ForgotPassword, ResetPassword
- ResendEmailVerification, ChangePassword, UpdateProfile, GetMe

**Onboarding (1 endpoint)**

- CompleteOnboarding (clinic setup wizard)

**Chronic Diseases (5 endpoints)**

- GetAll, GetPaginated, GetById, Create, Update, Delete

**Patient Chronic Diseases (4 endpoints)**

- GetChronicDiseases, AddChronicDisease, UpdateChronicDisease, RemoveChronicDisease

**Appointments (3 endpoints)**

- GetAppointments, CreateAppointment, GetAppointmentTypes

**Specializations (2 endpoints)**

- GetSpecializations, GetSpecialization

**Measurements (2 endpoints)**

- GetMeasurementAttributes, CreateMeasurementAttribute

**Medicines (5 endpoints)**

- GetMedicines, GetMedicine, CreateMedicine, UpdateMedicine, DeleteMedicine

**Medical Supplies (2 endpoints)**

- GetMedicalSupplies, CreateMedicalSupply

**Medical Services (2 endpoints)**

- GetMedicalServices, CreateMedicalService

**Invoices (2 endpoints)**

- GetInvoices, CreateInvoice

**Payments (1 endpoint)**

- CreatePayment

**System (1 endpoint)**

- SeedData (development/testing)

## 🚀 **Quick Start**

```bash
# Prerequisites: .NET 10, SQL Server LocalDB
git clone https://github.com/SherifOthman/clinic-api.git
cd clinic-api
dotnet restore
dotnet ef database update --project src/ClinicManagement.Infrastructure
dotnet run --project src/ClinicManagement.API
# Access: http://localhost:5000/swagger
```

**Default Super Admin:**

- Email: `superadmin@clinic.com`
- Password: `SuperAdmin123!`

## 🎯 **Subscription Plans**

branding, priority support, integrations

## 📊 **Project Stats**

- **Controllers**: 12
- **Entities**: 37
- **API Endpoints**: 35+
- **CQRS Handlers**: 50+
- **Background Services**: 1 (refresh token cleanup)
- **Reference Data Items**: 40+ (specializations, diseases, measurements, etc.)

---

**Demonstrates:** Modern .NET development, Clean Architecture, Multi-tenancy, CQRS, Healthcare domain modeling, Comprehensive business logic**Basic Plan** - $99/month

- 3 branches, 10 staff, 1,000 patients/month, 500 appointments/month
- 10GB storage, inventory management, reporting, backup & restore

**Professional Plan** - $199/month

- 10 branches, 50 staff, 10,000 patients/month, 2,000 appointments/month
- 50GB storage, API access, custom
