# Authorization Model

## Roles

### SuperAdmin

- System-wide administrator
- Can manage all clinics and users
- No clinic association

### ClinicOwner

- Owns and manages a specific clinic
- Full access to clinic data and settings
- Can manage staff, inventory, patients, appointments

### Doctor

- Medical staff member
- Can view and manage patients
- Can view appointments and medical records
- Can manage prescriptions and medical services

### Receptionist

- Front desk staff member
- Can manage appointments
- Can view patient basic information
- Limited access to medical records

## Authorization Policies

### SuperAdminOnly

- **Roles**: SuperAdmin
- **Use**: System administration endpoints

### ClinicManagement

- **Roles**: ClinicOwner
- **Use**: Clinic settings, staff management, onboarding

### MedicalStaff

- **Roles**: Doctor, ClinicOwner
- **Use**: Medical records, prescriptions, diagnoses

### StaffAccess

- **Roles**: Doctor, Receptionist, ClinicOwner
- **Use**: Appointments, patient basic info

### InventoryManagement

- **Roles**: ClinicOwner, Doctor
- **Use**: Medicines, medical supplies, medical services

## Endpoint Authorization Matrix

| Feature                 | SuperAdmin | ClinicOwner | Doctor | Receptionist |
| ----------------------- | ---------- | ----------- | ------ | ------------ |
| **Patients**            |
| View Patients           | ✅         | ✅          | ✅     | ✅           |
| Create Patient          | ✅         | ✅          | ✅     | ✅           |
| Update Patient          | ✅         | ✅          | ✅     | ✅           |
| Delete Patient          | ✅         | ✅          | ✅     | ❌           |
| **Medical Records**     |
| View Medical Records    | ✅         | ✅          | ✅     | ❌           |
| Create Medical Records  | ✅         | ✅          | ✅     | ❌           |
| **Inventory**           |
| View Medicines          | ✅         | ✅          | ✅     | ❌           |
| Manage Medicines        | ✅         | ✅          | ✅     | ❌           |
| View Medical Supplies   | ✅         | ✅          | ✅     | ❌           |
| Manage Medical Supplies | ✅         | ✅          | ✅     | ❌           |
| View Medical Services   | ✅         | ✅          | ✅     | ❌           |
| Manage Medical Services | ✅         | ✅          | ✅     | ❌           |
| **Appointments**        |
| View Appointments       | ✅         | ✅          | ✅     | ✅           |
| Manage Appointments     | ✅         | ✅          | ✅     | ✅           |
| **Billing**             |
| View Invoices           | ✅         | ✅          | ✅     | ✅           |
| Create Invoices         | ✅         | ✅          | ✅     | ✅           |
| Manage Payments         | ✅         | ✅          | ✅     | ✅           |
| **Clinic Management**   |
| Onboarding              | ✅         | ✅          | ❌     | ❌           |
| Clinic Settings         | ✅         | ✅          | ❌     | ❌           |
| Staff Management        | ✅         | ✅          | ❌     | ❌           |

## Implementation Notes

1. **Multi-tenancy**: All endpoints automatically filter by ClinicId (except SuperAdmin)
2. **Hierarchical Access**: ClinicOwner has all permissions within their clinic
3. **Flexible Policies**: Use policies for complex authorization rules
4. **Frontend Sync**: Frontend role checks must match backend authorization
