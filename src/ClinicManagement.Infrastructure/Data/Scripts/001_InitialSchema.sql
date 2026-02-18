-- Users Table (Identity with int ID)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserName NVARCHAR(256) NOT NULL,
        NormalizedUserName NVARCHAR(256) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        NormalizedEmail NVARCHAR(256) NOT NULL,
        EmailConfirmed BIT NOT NULL DEFAULT 0,
        PasswordHash NVARCHAR(MAX) NULL,
        SecurityStamp NVARCHAR(MAX) NULL,
        PhoneNumber NVARCHAR(50) NULL,
        PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        ProfileImageUrl NVARCHAR(500) NULL,
        CONSTRAINT UQ_Users_NormalizedUserName UNIQUE (NormalizedUserName),
        CONSTRAINT UQ_Users_NormalizedEmail UNIQUE (NormalizedEmail)
    );
    
    CREATE INDEX IX_Users_NormalizedUserName ON Users(NormalizedUserName);
    CREATE INDEX IX_Users_NormalizedEmail ON Users(NormalizedEmail);
END
GO

-- Roles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(256) NOT NULL,
        NormalizedName NVARCHAR(256) NOT NULL,
        CONSTRAINT UQ_Roles_NormalizedName UNIQUE (NormalizedName)
    );
    
    CREATE INDEX IX_Roles_NormalizedName ON Roles(NormalizedName);
END
GO

-- UserRoles Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE UserRoles (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);
END
GO

-- UserClaims Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserClaims')
BEGIN
    CREATE TABLE UserClaims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_UserClaims_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_UserClaims_UserId ON UserClaims(UserId);
END
GO

-- UserLogins Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserLogins')
BEGIN
    CREATE TABLE UserLogins (
        LoginProvider NVARCHAR(128) NOT NULL,
        ProviderKey NVARCHAR(128) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId INT NOT NULL,
        PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_UserLogins_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_UserLogins_UserId ON UserLogins(UserId);
END
GO

-- UserTokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserTokens')
BEGIN
    CREATE TABLE UserTokens (
        UserId INT NOT NULL,
        LoginProvider NVARCHAR(128) NOT NULL,
        Name NVARCHAR(128) NOT NULL,
        Value NVARCHAR(MAX) NULL,
        PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_UserTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

-- RoleClaims Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RoleClaims')
BEGIN
    CREATE TABLE RoleClaims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_RoleClaims_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_RoleClaims_RoleId ON RoleClaims(RoleId);
END
GO

-- RefreshTokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        ExpiryTime DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CreatedByIp NVARCHAR(50) NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        RevokedAt DATETIME2 NULL,
        RevokedByIp NVARCHAR(50) NULL,
        ReplacedByToken NVARCHAR(500) NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
END
GO

-- SubscriptionPlans Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionPlans')
BEGIN
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
        IsActive BIT NOT NULL DEFAULT 1,
        IsPopular BIT NOT NULL DEFAULT 0,
        DisplayOrder INT NOT NULL DEFAULT 0
    );
END
GO
