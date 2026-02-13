using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "AppointmentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ColorCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChronicDiseases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChronicDiseases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medication", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    YearlyFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SetupFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxBranches = table.Column<int>(type: "int", nullable: false),
                    MaxStaff = table.Column<int>(type: "int", nullable: false),
                    MaxPatientsPerMonth = table.Column<int>(type: "int", nullable: false),
                    MaxAppointmentsPerMonth = table.Column<int>(type: "int", nullable: false),
                    MaxInvoicesPerMonth = table.Column<int>(type: "int", nullable: false),
                    StorageLimitGB = table.Column<int>(type: "int", nullable: false),
                    HasInventoryManagement = table.Column<bool>(type: "bit", nullable: false),
                    HasReporting = table.Column<bool>(type: "bit", nullable: false),
                    HasAdvancedReporting = table.Column<bool>(type: "bit", nullable: false),
                    HasApiAccess = table.Column<bool>(type: "bit", nullable: false),
                    HasMultipleBranches = table.Column<bool>(type: "bit", nullable: false),
                    HasCustomBranding = table.Column<bool>(type: "bit", nullable: false),
                    HasPrioritySupport = table.Column<bool>(type: "bit", nullable: false),
                    HasBackupAndRestore = table.Column<bool>(type: "bit", nullable: false),
                    HasIntegrations = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecializationMeasurementAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultDisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DefaultIsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecializationMeasurementAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecializationMeasurementAttributes_MeasurementAttributes_MeasurementAttributeId",
                        column: x => x.MeasurementAttributeId,
                        principalTable: "MeasurementAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpecializationMeasurementAttributes_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QueueNumber = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AppointmentTypes_AppointmentTypeId",
                        column: x => x.AppointmentTypeId,
                        principalTable: "AppointmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicBranchAppointmentPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicBranchAppointmentPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicBranchAppointmentPrices_AppointmentTypes_AppointmentTypeId",
                        column: x => x.AppointmentTypeId,
                        principalTable: "AppointmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryGeoNameId = table.Column<int>(type: "int", nullable: false),
                    StateGeoNameId = table.Column<int>(type: "int", nullable: false),
                    CityGeoNameId = table.Column<int>(type: "int", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicBranches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClinicBranchPhoneNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicBranchPhoneNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicBranchPhoneNumbers_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsOperation = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalServices_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalSupplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    MinimumStockLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSupplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalSupplies_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BoxPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StripsPerBox = table.Column<int>(type: "int", nullable: false),
                    TotalStripsInStock = table.Column<int>(type: "int", nullable: false),
                    MinimumStockLevel = table.Column<int>(type: "int", nullable: false),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDiscontinued = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicines_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicMedication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicMedication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicMedication_Medication_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medication",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClinicOwners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessLicense = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OwnershipPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabTest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTest_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    CityGeoNameId = table.Column<int>(type: "int", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BloodType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KnownAllergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RadiologyTest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiologyTest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadiologyTest_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OnboardingCompleted = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientAllergy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllergyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiagnosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAllergy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientAllergy_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientChronicDiseases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChronicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientChronicDiseases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientChronicDiseases_ChronicDiseases_ChronicDiseaseId",
                        column: x => x.ChronicDiseaseId,
                        principalTable: "ChronicDiseases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientChronicDiseases_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPhones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPhones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPhones_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    YearsOfExperience = table.Column<short>(type: "smallint", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AvailableForEmergency = table.Column<bool>(type: "bit", nullable: false),
                    Biography = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doctors_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receptionists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShiftPreference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CanHandlePayments = table.Column<bool>(type: "bit", nullable: false),
                    Languages = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receptionists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receptionists_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffInvitations_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffInvitations_Users_AcceptedByUserId",
                        column: x => x.AcceptedByUserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffInvitations_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "Identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorMeasurementAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorMeasurementAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorMeasurementAttributes_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorMeasurementAttributes_MeasurementAttributes_MeasurementAttributeId",
                        column: x => x.MeasurementAttributeId,
                        principalTable: "MeasurementAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorWorkingDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorWorkingDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorWorkingDays_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorWorkingDays_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalVisit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalVisit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalVisit_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalVisit_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalVisit_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalVisit_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppointmentId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Appointments_AppointmentId1",
                        column: x => x.AppointmentId1,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabTestOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResultsAvailableAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultsUploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResultsFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResultsText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsAbnormal = table.Column<bool>(type: "bit", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTestOrder_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabTestOrder_Doctors_OrderedByDoctorId",
                        column: x => x.OrderedByDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabTestOrder_LabTest_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabTestOrder_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabTestOrder_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicalFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalFileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<byte>(type: "tinyint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalFiles_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalFiles_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalVisitLabTest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalVisitLabTest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalVisitLabTest_LabTest_LabTestId",
                        column: x => x.LabTestId,
                        principalTable: "LabTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalVisitLabTest_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalVisitMeasurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementAttributeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StringValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IntValue = table.Column<int>(type: "int", nullable: true),
                    DecimalValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DateTimeValue = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BooleanValue = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalVisitMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalVisitMeasurements_MeasurementAttributes_MeasurementAttributeId",
                        column: x => x.MeasurementAttributeId,
                        principalTable: "MeasurementAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalVisitMeasurements_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalVisitRadiology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RadiologyTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalVisitRadiology", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalVisitRadiology_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalVisitRadiology_RadiologyTest_RadiologyTestId",
                        column: x => x.RadiologyTestId,
                        principalTable: "RadiologyTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescription_MedicalVisit_VisitId",
                        column: x => x.VisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RadiologyOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RadiologyTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResultsAvailableAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultsUploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImageFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Findings = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadiologyOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadiologyOrder_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiologyOrder_Doctors_OrderedByDoctorId",
                        column: x => x.OrderedByDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiologyOrder_MedicalVisit_MedicalVisitId",
                        column: x => x.MedicalVisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiologyOrder_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadiologyOrder_RadiologyTest_RadiologyTestId",
                        column: x => x.RadiologyTestId,
                        principalTable: "RadiologyTest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicineDispensing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicBranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrescriptionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DispensedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineDispensing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineDispensing_ClinicBranches_ClinicBranchId",
                        column: x => x.ClinicBranchId,
                        principalTable: "ClinicBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicineDispensing_MedicalVisit_VisitId",
                        column: x => x.VisitId,
                        principalTable: "MedicalVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicineDispensing_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicineDispensing_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicineDispensing_Prescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicineDispensing_Users_DispensedByUserId",
                        column: x => x.DispensedByUserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FrequencyPerDay = table.Column<int>(type: "int", nullable: false),
                    DurationInDays = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItem_Prescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicalSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MedicineDispensingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LabTestOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RadiologyOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaleUnit = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_LabTestOrder_LabTestOrderId",
                        column: x => x.LabTestOrderId,
                        principalTable: "LabTestOrder",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceItems_MedicalServices_MedicalServiceId",
                        column: x => x.MedicalServiceId,
                        principalTable: "MedicalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_MedicalSupplies_MedicalSupplyId",
                        column: x => x.MedicalSupplyId,
                        principalTable: "MedicalSupplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_MedicineDispensing_MedicineDispensingId",
                        column: x => x.MedicineDispensingId,
                        principalTable: "MedicineDispensing",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_RadiologyOrder_RadiologyOrderId",
                        column: x => x.RadiologyOrderId,
                        principalTable: "RadiologyOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AppointmentTypes",
                columns: new[] { "Id", "ColorCode", "DescriptionAr", "DescriptionEn", "DisplayOrder", "IsActive", "NameAr", "NameEn" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#4CAF50", "كشف أول مرة مع الطبيب", "First time consultation with the doctor", 1, true, "كشف", "Consultation" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#2196F3", "زيارة متابعة للمرضى الحاليين", "Follow-up visit for existing patients", 2, true, "إعادة", "Follow-up" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#FF9800", "جلسة علاج أو معالجة", "Therapy or treatment session", 3, true, "جلسة", "Therapy Session" }
                });

            migrationBuilder.InsertData(
                table: "ChronicDiseases",
                columns: new[] { "Id", "DescriptionAr", "DescriptionEn", "NameAr", "NameEn" },
                values: new object[,]
                {
                    { new Guid("cd111111-1111-1111-1111-111111111111"), "حالة مزمنة لا ينتج فيها البنكرياس الأنسولين أو ينتج القليل منه", "A chronic condition in which the pancreas produces little or no insulin", "السكري النوع الأول", "Diabetes Type 1" },
                    { new Guid("cd222222-2222-2222-2222-222222222222"), "حالة مزمنة تؤثر على طريقة معالجة الجسم لسكر الدم", "A chronic condition that affects the way the body processes blood sugar", "السكري النوع الثاني", "Diabetes Type 2" },
                    { new Guid("cd333333-3333-3333-3333-333333333333"), "حالة ترتفع فيها ضغط الأوعية الدموية بشكل مستمر", "A condition in which the blood vessels have persistently raised pressure", "ارتفاع ضغط الدم", "Hypertension" },
                    { new Guid("cd444444-4444-4444-4444-444444444444"), "حالة تنفسية تتميز بنوبات تشنج في الشعب الهوائية", "A respiratory condition marked by attacks of spasm in the bronchi", "الربو", "Asthma" },
                    { new Guid("cd555555-5555-5555-5555-555555555555"), "حالة تتميز بفقدان تدريجي لوظائف الكلى مع مرور الوقت", "A condition characterized by a gradual loss of kidney function over time", "مرض الكلى المزمن", "Chronic Kidney Disease" },
                    { new Guid("cd666666-6666-6666-6666-666666666666"), "مجموعة من الحالات التي تؤثر على القلب", "A range of conditions that affect the heart", "أمراض القلب", "Heart Disease" },
                    { new Guid("cd777777-7777-7777-7777-777777777777"), "التهاب في مفصل واحد أو أكثر، يسبب الألم والتصلب", "Inflammation of one or more joints, causing pain and stiffness", "التهاب المفاصل", "Arthritis" },
                    { new Guid("cd888888-8888-8888-8888-888888888888"), "مرض الانسداد الرئوي المزمن - مجموعة من أمراض الرئة", "Chronic obstructive pulmonary disease - a group of lung conditions", "مرض الانسداد الرئوي المزمن", "COPD" },
                    { new Guid("cd999999-9999-9999-9999-999999999999"), "اضطراب في الصحة العقلية يتميز بالحزن المستمر", "A mental health disorder characterized by persistent sadness", "الاكتئاب", "Depression" },
                    { new Guid("cdaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "حالة طبية تنطوي على دهون الجسم المفرطة", "A medical condition involving excessive body fat", "السمنة", "Obesity" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentDate",
                table: "Appointments",
                column: "AppointmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentTypeId",
                table: "Appointments",
                column: "AppointmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClinicBranchId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "ClinicBranchId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_InvoiceId",
                table: "Appointments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId_AppointmentDate",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status",
                table: "Appointments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_DisplayOrder",
                table: "AppointmentTypes",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_IsActive",
                table: "AppointmentTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_NameAr",
                table: "AppointmentTypes",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentTypes_NameEn",
                table: "AppointmentTypes",
                column: "NameEn");

            migrationBuilder.CreateIndex(
                name: "IX_ChronicDiseases_NameAr",
                table: "ChronicDiseases",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_ChronicDiseases_NameEn",
                table: "ChronicDiseases",
                column: "NameEn");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranchAppointmentPrices_AppointmentTypeId",
                table: "ClinicBranchAppointmentPrices",
                column: "AppointmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranchAppointmentPrices_ClinicBranchId",
                table: "ClinicBranchAppointmentPrices",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranchAppointmentPrices_ClinicBranchId_AppointmentTypeId",
                table: "ClinicBranchAppointmentPrices",
                columns: new[] { "ClinicBranchId", "AppointmentTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranchAppointmentPrices_IsActive",
                table: "ClinicBranchAppointmentPrices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranches_CityGeoNameId",
                table: "ClinicBranches",
                column: "CityGeoNameId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranches_ClinicId",
                table: "ClinicBranches",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranches_CountryGeoNameId",
                table: "ClinicBranches",
                column: "CountryGeoNameId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranches_StateGeoNameId",
                table: "ClinicBranches",
                column: "StateGeoNameId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranchPhoneNumbers_ClinicBranchId",
                table: "ClinicBranchPhoneNumbers",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicBranchPhoneNumbers_PhoneNumber",
                table: "ClinicBranchPhoneNumbers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMedication_ClinicId",
                table: "ClinicMedication",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMedication_MedicationId",
                table: "ClinicMedication",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicOwners_UserId",
                table: "ClinicOwners",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_Name",
                table: "Clinics",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_OwnerUserId",
                table: "Clinics",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_SubscriptionPlanId",
                table: "Clinics",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMeasurementAttributes_DoctorId_MeasurementAttributeId",
                table: "DoctorMeasurementAttributes",
                columns: new[] { "DoctorId", "MeasurementAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMeasurementAttributes_MeasurementAttributeId",
                table: "DoctorMeasurementAttributes",
                column: "MeasurementAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecializationId",
                table: "Doctors",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorWorkingDays_ClinicBranchId",
                table: "DoctorWorkingDays",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorWorkingDays_Doctor_Branch_Day",
                table: "DoctorWorkingDays",
                columns: new[] { "DoctorId", "ClinicBranchId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_LabTestOrderId",
                table: "InvoiceItems",
                column: "LabTestOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_MedicalServiceId",
                table: "InvoiceItems",
                column: "MedicalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_MedicalSupplyId",
                table: "InvoiceItems",
                column: "MedicalSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_MedicineDispensingId",
                table: "InvoiceItems",
                column: "MedicineDispensingId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_MedicineId",
                table: "InvoiceItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_RadiologyOrderId",
                table: "InvoiceItems",
                column: "RadiologyOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId1",
                table: "Invoices",
                column: "AppointmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClinicId_InvoiceNumber",
                table: "Invoices",
                columns: new[] { "ClinicId", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_MedicalVisitId",
                table: "Invoices",
                column: "MedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTest_ClinicId",
                table: "LabTest",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestOrder_ClinicBranchId",
                table: "LabTestOrder",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestOrder_LabTestId",
                table: "LabTestOrder",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestOrder_MedicalVisitId",
                table: "LabTestOrder",
                column: "MedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestOrder_OrderedByDoctorId",
                table: "LabTestOrder",
                column: "OrderedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestOrder_PatientId",
                table: "LabTestOrder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementAttributes_NameAr",
                table: "MeasurementAttributes",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementAttributes_NameEn",
                table: "MeasurementAttributes",
                column: "NameEn");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalFiles_MedicalVisitId",
                table: "MedicalFiles",
                column: "MedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalFiles_PatientId",
                table: "MedicalFiles",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalServices_ClinicBranchId_Name",
                table: "MedicalServices",
                columns: new[] { "ClinicBranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSupplies_ClinicBranchId_Name",
                table: "MedicalSupplies",
                columns: new[] { "ClinicBranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisit_AppointmentId",
                table: "MedicalVisit",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisit_ClinicBranchId",
                table: "MedicalVisit",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisit_DoctorId",
                table: "MedicalVisit",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisit_PatientId",
                table: "MedicalVisit",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitLabTest_LabTestId",
                table: "MedicalVisitLabTest",
                column: "LabTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitLabTest_MedicalVisitId",
                table: "MedicalVisitLabTest",
                column: "MedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitMeasurements_MeasurementAttributeId",
                table: "MedicalVisitMeasurements",
                column: "MeasurementAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitMeasurements_MedicalVisitId_MeasurementAttributeId",
                table: "MedicalVisitMeasurements",
                columns: new[] { "MedicalVisitId", "MeasurementAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitRadiology_MedicalVisitId",
                table: "MedicalVisitRadiology",
                column: "MedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalVisitRadiology_RadiologyTestId",
                table: "MedicalVisitRadiology",
                column: "RadiologyTestId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_ClinicBranchId",
                table: "MedicineDispensing",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_DispensedByUserId",
                table: "MedicineDispensing",
                column: "DispensedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_MedicineId",
                table: "MedicineDispensing",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_PatientId",
                table: "MedicineDispensing",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_PrescriptionId",
                table: "MedicineDispensing",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineDispensing_VisitId",
                table: "MedicineDispensing",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_ClinicBranchId_Name",
                table: "Medicines",
                columns: new[] { "ClinicBranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergy_PatientId",
                table: "PatientAllergy",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientChronicDiseases_ChronicDiseaseId",
                table: "PatientChronicDiseases",
                column: "ChronicDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientChronicDiseases_PatientId",
                table: "PatientChronicDiseases",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientChronicDiseases_PatientId_ChronicDiseaseId",
                table: "PatientChronicDiseases",
                columns: new[] { "PatientId", "ChronicDiseaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientPhones_PatientId",
                table: "PatientPhones",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPhones_PhoneNumber",
                table: "PatientPhones",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CityGeoNameId",
                table: "Patients",
                column: "CityGeoNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ClinicId",
                table: "Patients",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientCode",
                table: "Patients",
                column: "PatientCode");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescription_VisitId",
                table: "Prescription",
                column: "VisitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItem_PrescriptionId",
                table: "PrescriptionItem",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiologyOrder_ClinicBranchId",
                table: "RadiologyOrder",
                column: "ClinicBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiologyOrder_MedicalVisitId",
                table: "RadiologyOrder",
                column: "MedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiologyOrder_OrderedByDoctorId",
                table: "RadiologyOrder",
                column: "OrderedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiologyOrder_PatientId",
                table: "RadiologyOrder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiologyOrder_RadiologyTestId",
                table: "RadiologyOrder",
                column: "RadiologyTestId");

            migrationBuilder.CreateIndex(
                name: "IX_RadiologyTest_ClinicId",
                table: "RadiologyTest",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Receptionists_UserId",
                table: "Receptionists",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiryTime",
                table: "RefreshTokens",
                column: "ExpiryTime");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "Identity",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "Identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SpecializationMeasurementAttributes_MeasurementAttributeId",
                table: "SpecializationMeasurementAttributes",
                column: "MeasurementAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecializationMeasurementAttributes_SpecializationId_MeasurementAttributeId",
                table: "SpecializationMeasurementAttributes",
                columns: new[] { "SpecializationId", "MeasurementAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_IsActive",
                table: "Specializations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_NameAr",
                table: "Specializations",
                column: "NameAr");

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_NameEn",
                table: "Specializations",
                column: "NameEn");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_AcceptedByUserId",
                table: "StaffInvitations",
                column: "AcceptedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_ClinicId",
                table: "StaffInvitations",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_Email_ClinicId_IsAccepted",
                table: "StaffInvitations",
                columns: new[] { "Email", "ClinicId", "IsAccepted" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_InvitedByUserId",
                table: "StaffInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_Token",
                table: "StaffInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "Identity",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "Identity",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "Identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "Identity",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClinicId",
                schema: "Identity",
                table: "Users",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "Identity",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                schema: "Identity",
                table: "Users",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserType",
                schema: "Identity",
                table: "Users",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "Identity",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ClinicBranches_ClinicBranchId",
                table: "Appointments",
                column: "ClinicBranchId",
                principalTable: "ClinicBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Invoices_InvoiceId",
                table: "Appointments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicBranchAppointmentPrices_ClinicBranches_ClinicBranchId",
                table: "ClinicBranchAppointmentPrices",
                column: "ClinicBranchId",
                principalTable: "ClinicBranches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicBranches_Clinics_ClinicId",
                table: "ClinicBranches",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicMedication_Clinics_ClinicId",
                table: "ClinicMedication",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicOwners_Users_UserId",
                table: "ClinicOwners",
                column: "UserId",
                principalSchema: "Identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clinics_Users_OwnerUserId",
                table: "Clinics",
                column: "OwnerUserId",
                principalSchema: "Identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ===== PERFORMANCE INDEXES FOR QUERY FILTERS =====
            
            // Soft delete filtered indexes (only index non-deleted records)
            // Simple indexes without INCLUDE to avoid column name issues
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_Patients_IsDeleted_Filtered] 
                ON [Patients]([IsDeleted], [ClinicId]) 
                WHERE [IsDeleted] = 0;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_Invoices_IsDeleted_Filtered] 
                ON [Invoices]([IsDeleted], [ClinicId], [Status]) 
                WHERE [IsDeleted] = 0;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_Appointments_IsDeleted_Filtered] 
                ON [Appointments]([IsDeleted], [AppointmentDate]) 
                WHERE [IsDeleted] = 0;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_Medicines_IsDeleted_Filtered] 
                ON [Medicines]([IsDeleted]) 
                WHERE [IsDeleted] = 0;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_ClinicBranches_IsDeleted_Filtered] 
                ON [ClinicBranches]([IsDeleted], [ClinicId]) 
                WHERE [IsDeleted] = 0;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_MedicalServices_IsDeleted_Filtered] 
                ON [MedicalServices]([IsDeleted]) 
                WHERE [IsDeleted] = 0;
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_MedicalSupplies_IsDeleted_Filtered] 
                ON [MedicalSupplies]([IsDeleted]) 
                WHERE [IsDeleted] = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ===== DROP PERFORMANCE INDEXES =====
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Patients_IsDeleted_Filtered] ON [Patients];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Invoices_IsDeleted_Filtered] ON [Invoices];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Appointments_IsDeleted_Filtered] ON [Appointments];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_Medicines_IsDeleted_Filtered] ON [Medicines];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_ClinicBranches_IsDeleted_Filtered] ON [ClinicBranches];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MedicalServices_IsDeleted_Filtered] ON [MedicalServices];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS [IX_MedicalSupplies_IsDeleted_Filtered] ON [MedicalSupplies];");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AppointmentTypes_AppointmentTypeId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ClinicBranches_ClinicBranchId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalVisit_ClinicBranches_ClinicBranchId",
                table: "MedicalVisit");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalVisit_Doctors_DoctorId",
                table: "MedicalVisit");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Invoices_InvoiceId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clinics_ClinicId",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ClinicBranchAppointmentPrices");

            migrationBuilder.DropTable(
                name: "ClinicBranchPhoneNumbers");

            migrationBuilder.DropTable(
                name: "ClinicMedication");

            migrationBuilder.DropTable(
                name: "ClinicOwners");

            migrationBuilder.DropTable(
                name: "DoctorMeasurementAttributes");

            migrationBuilder.DropTable(
                name: "DoctorWorkingDays");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "MedicalFiles");

            migrationBuilder.DropTable(
                name: "MedicalVisitLabTest");

            migrationBuilder.DropTable(
                name: "MedicalVisitMeasurements");

            migrationBuilder.DropTable(
                name: "MedicalVisitRadiology");

            migrationBuilder.DropTable(
                name: "PatientAllergy");

            migrationBuilder.DropTable(
                name: "PatientChronicDiseases");

            migrationBuilder.DropTable(
                name: "PatientPhones");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PrescriptionItem");

            migrationBuilder.DropTable(
                name: "Receptionists");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "SpecializationMeasurementAttributes");

            migrationBuilder.DropTable(
                name: "StaffInvitations");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Medication");

            migrationBuilder.DropTable(
                name: "LabTestOrder");

            migrationBuilder.DropTable(
                name: "MedicalServices");

            migrationBuilder.DropTable(
                name: "MedicalSupplies");

            migrationBuilder.DropTable(
                name: "MedicineDispensing");

            migrationBuilder.DropTable(
                name: "RadiologyOrder");

            migrationBuilder.DropTable(
                name: "ChronicDiseases");

            migrationBuilder.DropTable(
                name: "MeasurementAttributes");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "LabTest");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "Prescription");

            migrationBuilder.DropTable(
                name: "RadiologyTest");

            migrationBuilder.DropTable(
                name: "AppointmentTypes");

            migrationBuilder.DropTable(
                name: "ClinicBranches");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Specializations");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "MedicalVisit");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Identity");
        }
    }
}
