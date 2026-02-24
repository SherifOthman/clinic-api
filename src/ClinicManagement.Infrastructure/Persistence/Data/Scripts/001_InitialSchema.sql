-- =============================================
-- Clinic Management System - Initial Schema
-- All IDs are UNIQUEIDENTIFIER (GUID)
-- Generated in application code, not database
-- =============================================

-- =============================================
-- Reference Tables (Lookup Data)
-- =============================================

-- Roles table
CREATE TABLE Roles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Specializations table
CREATE TABLE Specializations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    NameEn NVARCHAR(200) NOT NULL,
    NameAr NVARCHAR(200) NOT NULL,
    DescriptionEn NVARCHAR(1000),
    DescriptionAr NVARCHAR(1000),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Chronic Diseases table
CREATE TABLE ChronicDiseases (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    NameEn NVARCHAR(200) NOT NULL,
    NameAr NVARCHAR(200) NOT NULL,
    DescriptionEn NVARCHAR(1000),
    DescriptionAr NVARCHAR(1000),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Subscription Plans table
CREATE TABLE SubscriptionPlans (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    NameAr NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    DescriptionAr NVARCHAR(500),
    MonthlyFee DECIMAL(18,2) NOT NULL,
    YearlyFee DECIMAL(18,2) NOT NULL,
    SetupFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxBranches INT NOT NULL,
    MaxStaff INT NOT NULL,
    MaxPatientsPerMonth INT NOT NULL,
    MaxAppointmentsPerMonth INT NOT NULL,
    MaxInvoicesPerMonth INT NOT NULL,
    StorageLimitGB INT NOT NULL,
    HasInventoryManagement BIT NOT NULL DEFAULT 0,
    HasReporting BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsPopular BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- =============================================
-- Identity & Authentication Tables
-- =============================================

-- Users table
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL UNIQUE,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    ProfileImageUrl NVARCHAR(500),
    IsEmailConfirmed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- UserRoles junction table
CREATE TABLE UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
);

-- RefreshTokens table
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiryTime DATETIME2 NOT NULL,
    CreatedByIp NVARCHAR(50) NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME2,
    RevokedByIp NVARCHAR(50),
    ReplacedByToken NVARCHAR(500),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- UserTokens table (for email confirmation, password reset)
CREATE TABLE UserTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    TokenType NVARCHAR(50) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- =============================================
-- Clinic & Organization Tables
-- =============================================

-- Clinics table
CREATE TABLE Clinics (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    OwnerUserId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanId UNIQUEIDENTIFIER NOT NULL,
    OnboardingCompleted BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (OwnerUserId) REFERENCES Users(Id),
    FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
);

-- ClinicBranches table
CREATE TABLE ClinicBranches (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    AddressLine NVARCHAR(500) NOT NULL,
    CountryGeoNameId INT NOT NULL,
    StateGeoNameId INT NOT NULL,
    CityGeoNameId INT NOT NULL,
    IsMainBranch BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE
);

-- ClinicBranchPhoneNumbers table
CREATE TABLE ClinicBranchPhoneNumbers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Label NVARCHAR(50),
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id) ON DELETE CASCADE
);

-- =============================================
-- Staff & Invitations Tables
-- =============================================

-- Staff table
CREATE TABLE Staff (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    HireDate DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE
);

-- DoctorProfiles table
CREATE TABLE DoctorProfiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    StaffId UNIQUEIDENTIFIER NOT NULL,
    SpecializationId UNIQUEIDENTIFIER NOT NULL,
    YearsOfExperience INT NOT NULL DEFAULT 0,
    FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE,
    FOREIGN KEY (SpecializationId) REFERENCES Specializations(Id)
);

-- StaffInvitation table
CREATE TABLE StaffInvitation (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    SpecializationId UNIQUEIDENTIFIER,
    InvitationToken NVARCHAR(100) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsAccepted BIT NOT NULL DEFAULT 0,
    IsCanceled BIT NOT NULL DEFAULT 0,
    AcceptedAt DATETIME2,
    AcceptedByUserId UNIQUEIDENTIFIER,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE,
    FOREIGN KEY (AcceptedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (SpecializationId) REFERENCES Specializations(Id)
);

-- DoctorWorkingDays table
CREATE TABLE DoctorWorkingDays (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DoctorId UNIQUEIDENTIFIER NOT NULL,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    Day INT NOT NULL, -- 0=Sunday, 6=Saturday
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id) ON DELETE CASCADE,
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id)
);

-- =============================================
-- Patient Tables
-- =============================================

-- Patients table
CREATE TABLE Patients (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    PatientCode NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    IsMale BIT NOT NULL,
    CityGeoNameId INT,
    DateOfBirth DATE NOT NULL,
    BloodType NVARCHAR(10),
    KnownAllergies NVARCHAR(MAX),
    EmergencyContactName NVARCHAR(200),
    EmergencyContactPhone NVARCHAR(20),
    EmergencyContactRelation NVARCHAR(50),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE
);

-- PatientPhones table
CREATE TABLE PatientPhones (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
);

-- PatientAllergies table
CREATE TABLE PatientAllergies (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    AllergyName NVARCHAR(200) NOT NULL,
    Severity INT NOT NULL, -- 0=Mild, 1=Moderate, 2=Severe
    Reaction NVARCHAR(500),
    DiagnosedAt DATE,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
);

-- PatientChronicDiseases junction table
CREATE TABLE PatientChronicDiseases (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    ChronicDiseaseId UNIQUEIDENTIFIER NOT NULL,
    DiagnosedAt DATE,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChronicDiseaseId) REFERENCES ChronicDiseases(Id)
);

-- =============================================
-- Appointment Tables
-- =============================================

-- AppointmentTypes table
CREATE TABLE AppointmentTypes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    NameEn NVARCHAR(100) NOT NULL,
    NameAr NVARCHAR(100) NOT NULL,
    DescriptionEn NVARCHAR(500),
    DescriptionAr NVARCHAR(500),
    DurationMinutes INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- ClinicBranchAppointmentPrices table
CREATE TABLE ClinicBranchAppointmentPrices (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    AppointmentTypeId UNIQUEIDENTIFIER NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id) ON DELETE CASCADE,
    FOREIGN KEY (AppointmentTypeId) REFERENCES AppointmentTypes(Id)
);

-- Appointments table
CREATE TABLE Appointments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AppointmentNumber NVARCHAR(50) NOT NULL UNIQUE,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    DoctorId UNIQUEIDENTIFIER NOT NULL,
    AppointmentTypeId UNIQUEIDENTIFIER NOT NULL,
    AppointmentDate DATETIME2 NOT NULL,
    QueueNumber SMALLINT NOT NULL,
    Status INT NOT NULL, -- 0=Pending, 1=Confirmed, 2=Completed, 3=Cancelled
    InvoiceId UNIQUEIDENTIFIER,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id),
    FOREIGN KEY (AppointmentTypeId) REFERENCES AppointmentTypes(Id)
);

-- =============================================
-- Medical Records Tables
-- =============================================

-- MeasurementAttributes table
CREATE TABLE MeasurementAttributes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    NameEn NVARCHAR(100) NOT NULL,
    NameAr NVARCHAR(100) NOT NULL,
    DescriptionEn NVARCHAR(500),
    DescriptionAr NVARCHAR(500),
    DataType INT NOT NULL, -- 0=String, 1=Int, 2=Decimal, 3=DateTime, 4=Boolean
    Unit NVARCHAR(20),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- SpecializationMeasurementAttributes junction table
CREATE TABLE SpecializationMeasurementAttributes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SpecializationId UNIQUEIDENTIFIER NOT NULL,
    MeasurementAttributeId UNIQUEIDENTIFIER NOT NULL,
    DefaultDisplayOrder INT NOT NULL DEFAULT 0,
    DefaultIsRequired BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (SpecializationId) REFERENCES Specializations(Id) ON DELETE CASCADE,
    FOREIGN KEY (MeasurementAttributeId) REFERENCES MeasurementAttributes(Id) ON DELETE CASCADE
);

-- DoctorMeasurementAttributes junction table
CREATE TABLE DoctorMeasurementAttributes (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DoctorId UNIQUEIDENTIFIER NOT NULL,
    MeasurementAttributeId UNIQUEIDENTIFIER NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsRequired BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id) ON DELETE CASCADE,
    FOREIGN KEY (MeasurementAttributeId) REFERENCES MeasurementAttributes(Id) ON DELETE CASCADE
);

-- MedicalVisits table
CREATE TABLE MedicalVisits (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    DoctorId UNIQUEIDENTIFIER NOT NULL,
    AppointmentId UNIQUEIDENTIFIER NOT NULL,
    Diagnosis NVARCHAR(MAX),
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (DoctorId) REFERENCES DoctorProfiles(Id),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id)
);

-- MedicalVisitMeasurements table
CREATE TABLE MedicalVisitMeasurements (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    MedicalVisitId UNIQUEIDENTIFIER NOT NULL,
    MeasurementAttributeId UNIQUEIDENTIFIER NOT NULL,
    StringValue NVARCHAR(500),
    IntValue INT,
    DecimalValue DECIMAL(18,4),
    DateTimeValue DATETIME2,
    BooleanValue BIT,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (MedicalVisitId) REFERENCES MedicalVisits(Id) ON DELETE CASCADE,
    FOREIGN KEY (MeasurementAttributeId) REFERENCES MeasurementAttributes(Id)
);

-- Prescriptions table
CREATE TABLE Prescriptions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PrescriptionNumber NVARCHAR(50) NOT NULL UNIQUE,
    VisitId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (VisitId) REFERENCES MedicalVisits(Id) ON DELETE CASCADE
);

-- PrescriptionItems table
CREATE TABLE PrescriptionItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    PrescriptionId UNIQUEIDENTIFIER NOT NULL,
    MedicationName NVARCHAR(200) NOT NULL,
    Dosage NVARCHAR(100),
    FrequencyPerDay INT NOT NULL,
    DurationInDays INT NOT NULL,
    Instructions NVARCHAR(MAX),
    FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id) ON DELETE CASCADE
);

-- LabTests table
CREATE TABLE LabTests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    NameEn NVARCHAR(200) NOT NULL,
    NameAr NVARCHAR(200) NOT NULL,
    DescriptionEn NVARCHAR(1000),
    DescriptionAr NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE
);

-- LabTestOrders table
CREATE TABLE LabTestOrders (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    LabTestId UNIQUEIDENTIFIER NOT NULL,
    MedicalVisitId UNIQUEIDENTIFIER,
    OrderedByDoctorId UNIQUEIDENTIFIER,
    Status INT NOT NULL, -- 0=Ordered, 1=InProgress, 2=ResultsAvailable, 3=Reviewed, 4=Cancelled
    OrderedAt DATETIME2 NOT NULL,
    PerformedAt DATETIME2,
    PerformedByUserId UNIQUEIDENTIFIER,
    ResultsAvailableAt DATETIME2,
    ResultsUploadedByUserId UNIQUEIDENTIFIER,
    ResultFilePath NVARCHAR(500),
    ResultNotes NVARCHAR(MAX),
    ReviewedAt DATETIME2,
    ReviewedByDoctorId UNIQUEIDENTIFIER,
    DoctorNotes NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (LabTestId) REFERENCES LabTests(Id),
    FOREIGN KEY (MedicalVisitId) REFERENCES MedicalVisits(Id),
    FOREIGN KEY (OrderedByDoctorId) REFERENCES DoctorProfiles(Id)
);

-- RadiologyTests table
CREATE TABLE RadiologyTests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    NameEn NVARCHAR(200) NOT NULL,
    NameAr NVARCHAR(200) NOT NULL,
    DescriptionEn NVARCHAR(1000),
    DescriptionAr NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE
);

-- RadiologyOrders table
CREATE TABLE RadiologyOrders (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    RadiologyTestId UNIQUEIDENTIFIER NOT NULL,
    MedicalVisitId UNIQUEIDENTIFIER,
    OrderedByDoctorId UNIQUEIDENTIFIER,
    Status INT NOT NULL,
    OrderedAt DATETIME2 NOT NULL,
    PerformedAt DATETIME2,
    PerformedByUserId UNIQUEIDENTIFIER,
    ResultsAvailableAt DATETIME2,
    ResultsUploadedByUserId UNIQUEIDENTIFIER,
    ImageFilePath NVARCHAR(500),
    ReportFilePath NVARCHAR(500),
    Findings NVARCHAR(MAX),
    ReviewedAt DATETIME2,
    ReviewedByDoctorId UNIQUEIDENTIFIER,
    DoctorNotes NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (RadiologyTestId) REFERENCES RadiologyTests(Id),
    FOREIGN KEY (MedicalVisitId) REFERENCES MedicalVisits(Id),
    FOREIGN KEY (OrderedByDoctorId) REFERENCES DoctorProfiles(Id)
);

-- =============================================
-- Billing Tables
-- =============================================

-- Invoices table
CREATE TABLE Invoices (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicId UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    AppointmentId UNIQUEIDENTIFIER,
    MedicalVisitId UNIQUEIDENTIFIER,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status INT NOT NULL, -- 0=Draft, 1=Issued, 2=PartiallyPaid, 3=FullyPaid, 4=Cancelled
    IssuedDate DATETIME2,
    DueDate DATETIME2,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id) ON DELETE CASCADE,
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (MedicalVisitId) REFERENCES MedicalVisits(Id)
);

-- InvoiceItems table
CREATE TABLE InvoiceItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InvoiceId UNIQUEIDENTIFIER NOT NULL,
    MedicalServiceId UNIQUEIDENTIFIER,
    MedicineId UNIQUEIDENTIFIER,
    MedicalSupplyId UNIQUEIDENTIFIER,
    MedicineDispensingId UNIQUEIDENTIFIER,
    LabTestOrderId UNIQUEIDENTIFIER,
    RadiologyOrderId UNIQUEIDENTIFIER,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    SaleUnit INT, -- 0=Box, 1=Strip (for medicines only)
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
);

-- Payments table
CREATE TABLE Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    InvoiceId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME2 NOT NULL,
    PaymentMethod INT NOT NULL, -- 0=Cash, 1=Card, 2=BankTransfer, 3=Insurance
    Status INT NOT NULL, -- 0=Paid, 1=Pending, 2=Failed, 3=Refunded
    Note NVARCHAR(MAX),
    ReferenceNumber NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
);

-- =============================================
-- Inventory Tables
-- =============================================

-- MedicalServices table
CREATE TABLE MedicalServices (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    NameEn NVARCHAR(200) NOT NULL,
    NameAr NVARCHAR(200) NOT NULL,
    DescriptionEn NVARCHAR(1000),
    DescriptionAr NVARCHAR(1000),
    DefaultPrice DECIMAL(18,2) NOT NULL,
    IsOperation BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id) ON DELETE CASCADE
);

-- MedicalSupplies table
CREATE TABLE MedicalSupplies (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    QuantityInStock INT NOT NULL,
    MinimumStockLevel INT NOT NULL DEFAULT 10,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id) ON DELETE CASCADE
);

-- Medicines table
CREATE TABLE Medicines (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Manufacturer NVARCHAR(200),
    BatchNumber NVARCHAR(100),
    ExpiryDate DATE,
    BoxPrice DECIMAL(18,2) NOT NULL,
    StripsPerBox INT NOT NULL,
    TotalStripsInStock INT NOT NULL,
    MinimumStockLevel INT NOT NULL DEFAULT 10,
    ReorderLevel INT NOT NULL DEFAULT 20,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDiscontinued BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id) ON DELETE CASCADE
);

-- MedicineDispensings table
CREATE TABLE MedicineDispensings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClinicBranchId UNIQUEIDENTIFIER NOT NULL,
    PatientId UNIQUEIDENTIFIER NOT NULL,
    MedicineId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DispensedAt DATETIME2 NOT NULL,
    DispensedByUserId UNIQUEIDENTIFIER NOT NULL,
    Status INT NOT NULL, -- 0=Dispensed, 1=PartiallyDispensed, 2=Cancelled
    Notes NVARCHAR(MAX),
    FOREIGN KEY (ClinicBranchId) REFERENCES ClinicBranches(Id),
    FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    FOREIGN KEY (MedicineId) REFERENCES Medicines(Id),
    FOREIGN KEY (DispensedByUserId) REFERENCES Users(Id)
);

-- =============================================
-- Indexes for Performance
-- =============================================

-- Users
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_UserName ON Users(UserName);

-- Clinics
CREATE INDEX IX_Clinics_OwnerUserId ON Clinics(OwnerUserId);
CREATE INDEX IX_ClinicBranches_ClinicId ON ClinicBranches(ClinicId);

-- Staff
CREATE INDEX IX_Staff_UserId ON Staff(UserId);
CREATE INDEX IX_Staff_ClinicId ON Staff(ClinicId);
CREATE INDEX IX_StaffInvitation_ClinicId ON StaffInvitation(ClinicId);
CREATE INDEX IX_StaffInvitation_Email ON StaffInvitation(Email);

-- Patients
CREATE INDEX IX_Patients_ClinicId ON Patients(ClinicId);
CREATE INDEX IX_Patients_PatientCode ON Patients(PatientCode);

-- Appointments
CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Appointments_DoctorId ON Appointments(DoctorId);
CREATE INDEX IX_Appointments_AppointmentDate ON Appointments(AppointmentDate);
CREATE INDEX IX_Appointments_ClinicBranchId ON Appointments(ClinicBranchId);

-- Medical Visits
CREATE INDEX IX_MedicalVisits_PatientId ON MedicalVisits(PatientId);
CREATE INDEX IX_MedicalVisits_DoctorId ON MedicalVisits(DoctorId);
CREATE INDEX IX_MedicalVisits_AppointmentId ON MedicalVisits(AppointmentId);

-- Invoices
CREATE INDEX IX_Invoices_ClinicId ON Invoices(ClinicId);
CREATE INDEX IX_Invoices_PatientId ON Invoices(PatientId);
CREATE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);

-- RefreshTokens
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);

GO

-- =============================================
-- Seed Data
-- =============================================

-- Seed Roles (with predefined GUIDs for consistency)
INSERT INTO Roles (Id, Name, Description) VALUES
(NEWID(), 'SuperAdmin', 'System administrator with full access'),
(NEWID(), 'ClinicOwner', 'Clinic owner with full clinic management access'),
(NEWID(), 'Doctor', 'Medical doctor providing patient care'),
(NEWID(), 'Receptionist', 'Front desk staff managing appointments and patients');

-- Seed Subscription Plans
INSERT INTO SubscriptionPlans (Id, Name, NameAr, Description, DescriptionAr, MonthlyFee, YearlyFee, SetupFee, MaxBranches, MaxStaff, MaxPatientsPerMonth, MaxAppointmentsPerMonth, MaxInvoicesPerMonth, StorageLimitGB, HasInventoryManagement, HasReporting, IsActive, IsPopular, DisplayOrder) VALUES
(NEWID(), 'Basic', N'أساسي', 'Perfect for small clinics', N'مثالي للعيادات الصغيرة', 99.00, 990.00, 0, 2, 5, 500, 300, 300, 10, 1, 1, 1, 0, 1),
(NEWID(), 'Professional', N'احترافي', 'Ideal for growing practices', N'مثالي للعيادات المتنامية', 199.00, 1990.00, 0, 5, 15, 2000, 1000, 1000, 50, 1, 1, 1, 1, 2),
(NEWID(), 'Enterprise', N'مؤسسي', 'For large medical facilities', N'للمنشآت الطبية الكبيرة', 399.00, 3990.00, 0, -1, -1, -1, -1, -1, 200, 1, 1, 1, 0, 3);

-- Seed Specializations
INSERT INTO Specializations (Id, NameEn, NameAr, DescriptionEn, DescriptionAr, IsActive) VALUES
(NEWID(), 'General Practice', N'طب عام', 'Primary care physicians providing comprehensive healthcare', N'أطباء الرعاية الأولية الذين يقدمون رعاية صحية شاملة', 1),
(NEWID(), 'Internal Medicine', N'الطب الباطني', 'Diagnosis and treatment of adult diseases', N'تشخيص وعلاج أمراض البالغين', 1),
(NEWID(), 'Pediatrics', N'طب الأطفال', 'Medical care for infants, children, and adolescents', N'الرعاية الطبية للرضع والأطفال والمراهقين', 1),
(NEWID(), 'Cardiology', N'أمراض القلب', 'Heart and cardiovascular system specialists', N'متخصصون في القلب والجهاز القلبي الوعائي', 1),
(NEWID(), 'Dermatology', N'الأمراض الجلدية', 'Skin, hair, and nail conditions', N'حالات الجلد والشعر والأظافر', 1),
(NEWID(), 'Orthopedics', N'جراحة العظام', 'Musculoskeletal system and injuries', N'الجهاز العضلي الهيكلي والإصابات', 1),
(NEWID(), 'Neurology', N'طب الأعصاب', 'Nervous system disorders', N'اضطرابات الجهاز العصبي', 1),
(NEWID(), 'Psychiatry', N'الطب النفسي', 'Mental health and behavioral disorders', N'الصحة النفسية والاضطرابات السلوكية', 1),
(NEWID(), 'Obstetrics & Gynecology', N'النساء والتوليد', 'Women''s reproductive health and pregnancy', N'صحة المرأة الإنجابية والحمل', 1),
(NEWID(), 'Ophthalmology', N'طب العيون', 'Eye and vision care', N'رعاية العين والبصر', 1),
(NEWID(), 'Otolaryngology (ENT)', N'الأنف والأذن والحنجرة', 'Ear, nose, and throat conditions', N'حالات الأذن والأنف والحنجرة', 1),
(NEWID(), 'Urology', N'المسالك البولية', 'Urinary tract and male reproductive system', N'الجهاز البولي والجهاز التناسلي الذكري', 1),
(NEWID(), 'Gastroenterology', N'الجهاز الهضمي', 'Digestive system disorders', N'اضطرابات الجهاز الهضمي', 1),
(NEWID(), 'Endocrinology', N'الغدد الصماء', 'Hormonal and metabolic disorders', N'الاضطرابات الهرمونية والأيضية', 1),
(NEWID(), 'Pulmonology', N'أمراض الرئة', 'Respiratory system and lung diseases', N'الجهاز التنفسي وأمراض الرئة', 1),
(NEWID(), 'Nephrology', N'أمراض الكلى', 'Kidney diseases and disorders', N'أمراض واضطرابات الكلى', 1),
(NEWID(), 'Rheumatology', N'أمراض الروماتيزم', 'Autoimmune and musculoskeletal diseases', N'أمراض المناعة الذاتية والعضلات الهيكلية', 1),
(NEWID(), 'Oncology', N'علم الأورام', 'Cancer diagnosis and treatment', N'تشخيص وعلاج السرطان', 1),
(NEWID(), 'Radiology', N'الأشعة', 'Medical imaging and diagnostics', N'التصوير الطبي والتشخيص', 1),
(NEWID(), 'Anesthesiology', N'التخدير', 'Anesthesia and pain management', N'التخدير وإدارة الألم', 1),
(NEWID(), 'Emergency Medicine', N'طب الطوارئ', 'Acute and emergency care', N'الرعاية الحادة والطارئة', 1),
(NEWID(), 'Family Medicine', N'طب الأسرة', 'Comprehensive care for all ages', N'رعاية شاملة لجميع الأعمار', 1),
(NEWID(), 'Sports Medicine', N'الطب الرياضي', 'Athletic injuries and performance', N'الإصابات الرياضية والأداء', 1),
(NEWID(), 'Allergy & Immunology', N'الحساسية والمناعة', 'Allergic and immune system disorders', N'اضطرابات الحساسية والجهاز المناعي', 1),
(NEWID(), 'Infectious Disease', N'الأمراض المعدية', 'Bacterial, viral, and parasitic infections', N'العدوى البكتيرية والفيروسية والطفيلية', 1);

PRINT 'Initial schema created and seeded successfully';
