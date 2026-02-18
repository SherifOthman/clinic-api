-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO Roles (Name)
    VALUES 
        ('SuperAdmin'),
        ('ClinicOwner'),
        ('Doctor'),
        ('Receptionist')
END
GO

-- SuperAdmin user will be seeded via SuperAdminSeedService with properly hashed password

-- Seed Subscription Plans
IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans)
BEGIN
    INSERT INTO SubscriptionPlans (
        Name, NameAr, Description, DescriptionAr, MonthlyFee, YearlyFee,
        SetupFee, MaxBranches, MaxStaff, MaxPatientsPerMonth, MaxAppointmentsPerMonth,
        MaxInvoicesPerMonth, StorageLimitGB, HasInventoryManagement, HasReporting,
        IsActive, IsPopular, DisplayOrder
    ) VALUES
    -- Starter Plan
    (
        'Starter Plan', 'الخطة المبتدئة',
        'Perfect for solo practitioners starting out',
        'مثالية للممارسين المفردين المبتدئين',
        49, 490, 0, 1, 3, 200, 150, 150, 5, 0, 1, 1, 0, 1
    ),
    -- Basic Plan (Most Popular)
    (
        'Basic Plan', 'الخطة الأساسية',
        'Essential features for small clinics',
        'الميزات الأساسية للعيادات الصغيرة',
        99, 990, 50, 3, 10, 1000, 500, 500, 10, 1, 1, 1, 1, 2
    ),
    -- Professional Plan
    (
        'Professional Plan', 'الخطة الاحترافية',
        'Advanced features for established clinics',
        'ميزات متقدمة للعيادات المتقدمة',
        199, 1990, 0, 10, 50, 10000, 2000, 2000, 50, 1, 1, 1, 0, 3
    ),
    -- Enterprise Plan
    (
        'Enterprise Plan', 'خطة المؤسسات',
        'Complete solution for large healthcare organizations',
        'حل شامل للمؤسسات الصحية الكبيرة',
        399, 3990, 0, -1, -1, -1, -1, -1, 200, 1, 1, 1, 0, 4
    )
END
GO
