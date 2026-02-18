-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES 
        (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID()),
        (NEWID(), 'ClinicOwner', 'CLINICOWNER', NEWID()),
        (NEWID(), 'Doctor', 'DOCTOR', NEWID()),
        (NEWID(), 'Receptionist', 'RECEPTIONIST', NEWID())
END
GO

-- Seed Subscription Plans
IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans)
BEGIN
    INSERT INTO SubscriptionPlans (
        Id, Name, NameAr, Description, DescriptionAr, MonthlyFee, YearlyFee,
        SetupFee, MaxBranches, MaxStaff, MaxPatientsPerMonth, MaxAppointmentsPerMonth,
        MaxInvoicesPerMonth, StorageLimitGB, HasInventoryManagement, HasReporting,
        IsActive, IsPopular, DisplayOrder
    ) VALUES
    -- Starter Plan
    (
        NEWID(), 'Starter Plan', 'خطة المبتدئة',
        'Perfect for solo practitioners starting out',
        'مثالية للممارسين المفردين المبتدئين',
        49, 490, 0, 1, 3, 200, 150, 150, 5, 0, 1, 1, 0, 1
    ),
    -- Basic Plan (Most Popular)
    (
        NEWID(), 'Basic Plan', 'الخطة الأساسية',
        'Essential features for small clinics',
        'الميزات الأساسية للعيادات الصغيرة',
        99, 990, 50, 3, 10, 1000, 500, 500, 10, 1, 1, 1, 1, 2
    ),
    -- Professional Plan
    (
        NEWID(), 'Professional Plan', 'الخطة الاحترافية',
        'Advanced features for established clinics',
        'ميزات متقدمة للعيادات المتقدمة',
        199, 1990, 0, 10, 50, 10000, 2000, 2000, 50, 1, 1, 1, 0, 3
    ),
    -- Enterprise Plan
    (
        NEWID(), 'Enterprise Plan', 'خطة المؤسسات',
        'Complete solution for large healthcare organizations',
        'حل شامل للمؤسسات الصحية الكبيرة',
        399, 3990, 0, -1, -1, -1, -1, -1, 200, 1, 1, 1, 0, 4
    )
END
GO
