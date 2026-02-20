-- =============================================
-- Clinic Management System - Initial Schema
-- =============================================

-- Users Table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    ProfileImageUrl NVARCHAR(500) NULL,
    IsEmailConfirmed BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_UserName ON Users(UserName);

-- UserTokens Table (for email confirmation and password reset)
CREATE TABLE UserTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    TokenType NVARCHAR(50) NOT NULL,
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    UsedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_UserTokens_UserId ON UserTokens(UserId);
CREATE INDEX IX_UserTokens_Token ON UserTokens(Token);
CREATE INDEX IX_UserTokens_TokenType ON UserTokens(TokenType);
CREATE INDEX IX_UserTokens_IsUsed ON UserTokens(IsUsed);

-- Roles Table
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Roles_Name ON Roles(Name);

-- UserRoles Table
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- RefreshTokens Table
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiryTime DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedByIp NVARCHAR(50) NULL,
    RevokedAt DATETIME2 NULL,
    RevokedByIp NVARCHAR(50) NULL,
    ReplacedByToken NVARCHAR(500) NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
CREATE INDEX IX_RefreshTokens_IsRevoked ON RefreshTokens(IsRevoked);

-- SubscriptionPlans Table
CREATE TABLE SubscriptionPlans (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    NameAr NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    DescriptionAr NVARCHAR(500) NOT NULL,
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
    HasAdvancedReporting BIT NOT NULL DEFAULT 0,
    HasApiAccess BIT NOT NULL DEFAULT 0,
    HasMultipleBranches BIT NOT NULL DEFAULT 0,
    HasCustomBranding BIT NOT NULL DEFAULT 0,
    HasPrioritySupport BIT NOT NULL DEFAULT 0,
    HasBackupAndRestore BIT NOT NULL DEFAULT 0,
    HasIntegrations BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsPopular BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,

);

CREATE INDEX IX_SubscriptionPlans_IsActive ON SubscriptionPlans(IsActive);
CREATE INDEX IX_SubscriptionPlans_IsDeleted ON SubscriptionPlans(IsDeleted);

-- Clinics Table
CREATE TABLE Clinics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    OwnerUserId INT NOT NULL,
    SubscriptionPlanId INT NOT NULL,
    OnboardingCompleted BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (OwnerUserId) REFERENCES Users(Id),
    FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
);

CREATE INDEX IX_Clinics_OwnerUserId ON Clinics(OwnerUserId);
CREATE INDEX IX_Clinics_SubscriptionPlanId ON Clinics(SubscriptionPlanId);
CREATE INDEX IX_Clinics_IsDeleted ON Clinics(IsDeleted);

-- ClinicBranches Table
CREATE TABLE ClinicBranches (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClinicId INT NOT NULL,
    CountryGeoNameId INT NOT NULL,
    StateGeoNameId INT NOT NULL,
    CityGeoNameId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id)
);

CREATE INDEX IX_ClinicBranches_ClinicId ON ClinicBranches(ClinicId);
CREATE INDEX IX_ClinicBranches_IsDeleted ON ClinicBranches(IsDeleted);

-- Specializations Table
CREATE TABLE Specializations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NameEn NVARCHAR(200) NOT NULL,
    NameAr NVARCHAR(200) NOT NULL,
    DescriptionEn NVARCHAR(500) NULL,
    DescriptionAr NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Specializations_NameEn ON Specializations(NameEn);
CREATE INDEX IX_Specializations_NameAr ON Specializations(NameAr);
CREATE INDEX IX_Specializations_IsActive ON Specializations(IsActive);
CREATE INDEX IX_Specializations_IsDeleted ON Specializations(IsDeleted);


-- Staff Table
CREATE TABLE Staff (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ClinicId INT NOT NULL,
    HireDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id)
);

CREATE INDEX IX_Staff_UserId ON Staff(UserId);
CREATE INDEX IX_Staff_ClinicId ON Staff(ClinicId);
CREATE INDEX IX_Staff_IsActive ON Staff(IsActive);
CREATE INDEX IX_Staff_IsDeleted ON Staff(IsDeleted);

-- DoctorProfile Table
CREATE TABLE DoctorProfile (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL UNIQUE,
    SpecializationId INT NULL,
    YearsOfExperience INT NULL,
    FOREIGN KEY (StaffId) REFERENCES Staff(Id),
    FOREIGN KEY (SpecializationId) REFERENCES Specializations(Id)
);

CREATE INDEX IX_DoctorProfile_StaffId ON DoctorProfile(StaffId);
CREATE INDEX IX_DoctorProfile_SpecializationId ON DoctorProfile(SpecializationId);


-- StaffInvitation Table
CREATE TABLE StaffInvitation (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClinicId INT NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    InvitationToken NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsAccepted BIT NOT NULL DEFAULT 0,
    AcceptedAt DATETIME2 NULL,
    AcceptedByUserId INT NULL,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ClinicId) REFERENCES Clinics(Id),
    FOREIGN KEY (AcceptedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_StaffInvitation_ClinicId ON StaffInvitation(ClinicId);
CREATE INDEX IX_StaffInvitation_Email ON StaffInvitation(Email);
CREATE INDEX IX_StaffInvitation_InvitationToken ON StaffInvitation(InvitationToken);
CREATE INDEX IX_StaffInvitation_IsAccepted ON StaffInvitation(IsAccepted);
CREATE INDEX IX_StaffInvitation_IsDeleted ON StaffInvitation(IsDeleted);

-- =============================================
-- Seed Data
-- =============================================

-- Seed Roles
INSERT INTO Roles (Name, Description) VALUES
('SuperAdmin', 'System administrator with full access'),
('ClinicOwner', 'Clinic owner with full clinic management access'),
('Doctor', 'Medical doctor providing patient care'),
('Receptionist', 'Front desk staff managing appointments and patients');

-- Seed Subscription Plans
INSERT INTO SubscriptionPlans (Name, NameAr, Description, DescriptionAr, MonthlyFee, YearlyFee, SetupFee, MaxBranches, MaxStaff, MaxPatientsPerMonth, MaxAppointmentsPerMonth, MaxInvoicesPerMonth, StorageLimitGB, HasInventoryManagement, HasReporting, IsActive, IsPopular, DisplayOrder) VALUES
('Basic', N'أساسي', 'Perfect for small clinics', N'مثالي للعيادات الصغيرة', 99.00, 990.00, 0, 2, 5, 500, 300, 300, 10, 1, 1, 1, 0, 1),
('Professional', N'احترافي', 'Ideal for growing practices', N'مثالي للعيادات المتنامية', 199.00, 1990.00, 0, 5, 15, 2000, 1000, 1000, 50, 1, 1, 1, 1, 2),
('Enterprise', N'مؤسسي', 'For large medical facilities', N'للمنشآت الطبية الكبيرة', 399.00, 3990.00, 0, -1, -1, -1, -1, -1, 200, 1, 1, 1, 0, 3);

-- Seed Specializations
INSERT INTO Specializations (NameEn, NameAr, DescriptionEn, DescriptionAr, IsActive) VALUES
('General Practice', N'طب عام', 'Primary care physicians providing comprehensive healthcare', N'أطباء الرعاية الأولية الذين يقدمون رعاية صحية شاملة', 1),
('Internal Medicine', N'الطب الباطني', 'Diagnosis and treatment of adult diseases', N'تشخيص وعلاج أمراض البالغين', 1),
('Pediatrics', N'طب الأطفال', 'Medical care for infants, children, and adolescents', N'الرعاية الطبية للرضع والأطفال والمراهقين', 1),
('Cardiology', N'أمراض القلب', 'Heart and cardiovascular system specialists', N'متخصصون في القلب والجهاز القلبي الوعائي', 1),
('Dermatology', N'الأمراض الجلدية', 'Skin, hair, and nail conditions', N'حالات الجلد والشعر والأظافر', 1),
('Orthopedics', N'جراحة العظام', 'Musculoskeletal system and injuries', N'الجهاز العضلي الهيكلي والإصابات', 1),
('Neurology', N'طب الأعصاب', 'Nervous system disorders', N'اضطرابات الجهاز العصبي', 1),
('Psychiatry', N'الطب النفسي', 'Mental health and behavioral disorders', N'الصحة النفسية والاضطرابات السلوكية', 1),
('Obstetrics & Gynecology', N'النساء والتوليد', 'Women''s reproductive health and pregnancy', N'صحة المرأة الإنجابية والحمل', 1),
('Ophthalmology', N'طب العيون', 'Eye and vision care', N'رعاية العين والبصر', 1),
('Otolaryngology (ENT)', N'الأنف والأذن والحنجرة', 'Ear, nose, and throat conditions', N'حالات الأذن والأنف والحنجرة', 1),
('Urology', N'المسالك البولية', 'Urinary tract and male reproductive system', N'الجهاز البولي والجهاز التناسلي الذكري', 1),
('Gastroenterology', N'الجهاز الهضمي', 'Digestive system disorders', N'اضطرابات الجهاز الهضمي', 1),
('Endocrinology', N'الغدد الصماء', 'Hormonal and metabolic disorders', N'الاضطرابات الهرمونية والأيضية', 1),
('Pulmonology', N'أمراض الرئة', 'Respiratory system and lung diseases', N'الجهاز التنفسي وأمراض الرئة', 1),
('Nephrology', N'أمراض الكلى', 'Kidney diseases and disorders', N'أمراض واضطرابات الكلى', 1),
('Rheumatology', N'أمراض الروماتيزم', 'Autoimmune and musculoskeletal diseases', N'أمراض المناعة الذاتية والعضلات الهيكلية', 1),
('Oncology', N'علم الأورام', 'Cancer diagnosis and treatment', N'تشخيص وعلاج السرطان', 1),
('Radiology', N'الأشعة', 'Medical imaging and diagnostics', N'التصوير الطبي والتشخيص', 1),
('Anesthesiology', N'التخدير', 'Anesthesia and pain management', N'التخدير وإدارة الألم', 1),
('Emergency Medicine', N'طب الطوارئ', 'Acute and emergency care', N'الرعاية الحادة والطارئة', 1),
('Family Medicine', N'طب الأسرة', 'Comprehensive care for all ages', N'رعاية شاملة لجميع الأعمار', 1),
('Sports Medicine', N'الطب الرياضي', 'Athletic injuries and performance', N'الإصابات الرياضية والأداء', 1),
('Allergy & Immunology', N'الحساسية والمناعة', 'Allergic and immune system disorders', N'اضطرابات الحساسية والجهاز المناعي', 1),
('Infectious Disease', N'الأمراض المعدية', 'Bacterial, viral, and parasitic infections', N'العدوى البكتيرية والفيروسية والطفيلية', 1);

PRINT 'Initial schema created and seeded successfully';
