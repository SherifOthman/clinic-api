# Clinic Management System - API Documentation

## Base URL
```
Development: http://localhost:5000
Production: https://your-domain.com
```

## Authentication
Most endpoints require authentication using JWT Bearer tokens in the Authorization header:
```
Authorization: Bearer {access_token}
```

---

## Authentication Endpoints

### 1. Register User
**POST** `/auth/register`

Register a new user account.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "email": "user@example.com",
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe"
}
```

**Note:** User naming follows Arabic naming convention with firstName, secondName (optional), and thirdName.

**Response (400 Bad Request):**
```json
{
  "statusCode": 400,
  "message": "Email already exists",
  "details": null
}
```

---

### 2. Login
**POST** `/auth/login`

Authenticate user and receive access token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "secondName": "Michael",
    "thirdName": "Doe"
  }
}
```
*Note: Refresh token is set as HTTP-only cookie*

**Response (400 Bad Request):**
```json
{
  "statusCode": 400,
  "message": "Invalid credentials",
  "details": null
}
```

---

### 3. Refresh Token
**POST** `/auth/refresh-token`

Get a new access token using refresh token cookie.

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "secondName": "Michael",
    "thirdName": "Doe"
  }
}
```

**Response (401 Unauthorized):**
```json
{
  "statusCode": 401,
  "message": "Refresh token not found",
  "details": null
}
```

---

### 4. Logout
**POST** `/auth/logout`

Logout user and clear refresh token cookie.

**Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

---

## Clinic Endpoints

### 1. Get All Clinics
**GET** `/clinics`

Retrieve list of clinics with pagination and filtering.

**Query Parameters:**
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `searchTerm` (optional): Search by clinic name
- `isActive` (optional): Filter by active status

**Example:**
```
GET /clinics?pageNumber=1&pageSize=10&searchTerm=dental&isActive=true
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Dental Care Clinic",
      "phone": "+1234567890",
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": null,
      "isActive": true,
      "ownerId": 1,
      "subscriptionPlanId": 2
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 2. Get Clinic by ID
**GET** `/clinics/{id}`

Retrieve specific clinic details.

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Dental Care Clinic",
  "phone": "+1234567890",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": null,
  "isActive": true,
  "ownerId": 1,
  "subscriptionPlanId": 2,
  "branches": []
}
```

**Response (404 Not Found):**
```json
{
  "statusCode": 404,
  "message": "Clinic not found",
  "details": null
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 3. Create Clinic
**POST** `/clinics`

Create a new clinic.

**Request Body:**
```json
{
  "name": "Dental Care Clinic",
  "phone": "+1234567890",
  "ownerId": 1,
  "subscriptionPlanId": 2
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "name": "Dental Care Clinic",
  "phone": "+1234567890",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": null,
  "isActive": true,
  "ownerId": 1,
  "subscriptionPlanId": 2
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 4. Update Clinic
**PUT** `/clinics/{id}`

Update existing clinic.

**Request Body:**
```json
{
  "name": "Updated Clinic Name",
  "phone": "+1234567890",
  "isActive": true,
  "subscriptionPlanId": 2
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Updated Clinic Name",
  "phone": "+1234567890",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": null,
  "isActive": true,
  "ownerId": 1,
  "subscriptionPlanId": 2
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

## Patient Endpoints

### 1. Get All Patients
**GET** `/patients`

Retrieve list of patients with pagination and filtering.

**Query Parameters:**
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `clinicId` (optional): Filter by clinic
- `searchTerm` (optional): Search by name or phone

**Example:**
```
GET /patients?pageNumber=1&pageSize=10&clinicId=1&searchTerm=john
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": 1,
      "clinicId": 1,
      "avatar": null,
      "firstName": "John",
      "secondName": "Michael",
      "thirdName": "Doe",
      "dateOfBirth": "1990-05-15T00:00:00Z",
      "gender": "Male",
      "city": "New York",
      "phoneNumber": "+1234567890",
      "emergencyContactName": "Jane Doe",
      "emergencyPhone": "+1234567891",
      "generalNotes": "No allergies"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 2. Get Patient by ID
**GET** `/patients/{id}`

Retrieve specific patient details.

**Response (200 OK):**
```json
{
  "id": 1,
  "clinicId": 1,
  "avatar": null,
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "gender": "Male",
  "city": "New York",
  "phoneNumber": "+1234567890",
  "emergencyContactName": "Jane Doe",
  "emergencyPhone": "+1234567891",
  "generalNotes": "No allergies",
  "appointments": [],
  "surgeries": []
}
```

**Response (404 Not Found):**
```json
{
  "statusCode": 404,
  "message": "Patient not found",
  "details": null
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 3. Create Patient
**POST** `/patients`

Create a new patient.

**Request Body:**
```json
{
  "clinicId": 1,
  "avatar": null,
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "gender": "Male",
  "city": "New York",
  "phoneNumber": "+1234567890",
  "emergencyContactName": "Jane Doe",
  "emergencyPhone": "+1234567891",
  "generalNotes": "No allergies"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "clinicId": 1,
  "avatar": null,
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "gender": "Male",
  "city": "New York",
  "phoneNumber": "+1234567890",
  "emergencyContactName": "Jane Doe",
  "emergencyPhone": "+1234567891",
  "generalNotes": "No allergies"
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 4. Update Patient
**PUT** `/patients/{id}`

Update existing patient.

**Request Body:**
```json
{
  "avatar": null,
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "gender": "Male",
  "city": "New York",
  "phoneNumber": "+1234567890",
  "emergencyContactName": "Jane Doe",
  "emergencyPhone": "+1234567891",
  "generalNotes": "Updated notes"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "clinicId": 1,
  "avatar": null,
  "firstName": "John",
  "secondName": "Michael",
  "thirdName": "Doe",
  "dateOfBirth": "1990-05-15T00:00:00Z",
  "gender": "Male",
  "city": "New York",
  "phoneNumber": "+1234567890",
  "emergencyContactName": "Jane Doe",
  "emergencyPhone": "+1234567891",
  "generalNotes": "Updated notes"
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

## Appointment Endpoints

### 1. Get All Appointments
**GET** `/appointments`

Retrieve list of appointments with pagination and filtering.

**Query Parameters:**
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `clinicId` (optional): Filter by clinic
- `patientId` (optional): Filter by patient
- `doctorId` (optional): Filter by doctor
- `status` (optional): Filter by status (Scheduled, Confirmed, InProgress, Completed, Cancelled, Rescheduled, NoShow)
- `fromDate` (optional): Filter appointments from this date
- `toDate` (optional): Filter appointments to this date

**Example:**
```
GET /appointments?pageNumber=1&pageSize=10&clinicId=1&status=Scheduled
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": 1,
      "branchId": 1,
      "patientId": 1,
      "doctorId": 1,
      "receptionistId": 1,
      "status": "Scheduled",
      "type": "FirstVisit",
      "appointmentDate": "2024-02-01T10:00:00Z",
      "price": 100.00,
      "paidPrice": 50.00,
      "discount": 0.00,
      "notes": "Regular checkup"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 2. Get Appointment by ID
**GET** `/appointments/{id}`

Retrieve specific appointment details.

**Response (200 OK):**
```json
{
  "id": 1,
  "branchId": 1,
  "patientId": 1,
  "doctorId": 1,
  "receptionistId": 1,
  "status": "Scheduled",
  "type": "FirstVisit",
  "appointmentDate": "2024-02-01T10:00:00Z",
  "price": 100.00,
  "paidPrice": 50.00,
  "discount": 0.00,
  "notes": "Regular checkup",
  "patient": {
    "id": 1,
    "firstName": "John",
    "thirdName": "Doe"
  },
  "doctor": {
    "id": 1,
    "firstName": "Dr. Smith"
  }
}
```

**Response (404 Not Found):**
```json
{
  "statusCode": 404,
  "message": "Appointment not found",
  "details": null
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 3. Create Appointment
**POST** `/appointments`

Create a new appointment.

**Request Body:**
```json
{
  "branchId": 1,
  "patientId": 1,
  "doctorId": 1,
  "receptionistId": 1,
  "type": "FirstVisit",
  "appointmentDate": "2024-02-01T10:00:00Z",
  "price": 100.00,
  "paidPrice": 50.00,
  "discount": 0.00,
  "notes": "Regular checkup"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "branchId": 1,
  "patientId": 1,
  "doctorId": 1,
  "receptionistId": 1,
  "status": "Scheduled",
  "type": "FirstVisit",
  "appointmentDate": "2024-02-01T10:00:00Z",
  "price": 100.00,
  "paidPrice": 50.00,
  "discount": 0.00,
  "notes": "Regular checkup"
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

### 4. Update Appointment
**PUT** `/appointments/{id}`

Update existing appointment.

**Request Body:**
```json
{
  "status": "Confirmed",
  "appointmentDate": "2024-02-01T10:00:00Z",
  "price": 100.00,
  "paidPrice": 100.00,
  "discount": 0.00,
  "notes": "Updated notes"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "branchId": 1,
  "patientId": 1,
  "doctorId": 1,
  "receptionistId": 1,
  "status": "Confirmed",
  "type": "FirstVisit",
  "appointmentDate": "2024-02-01T10:00:00Z",
  "price": 100.00,
  "paidPrice": 100.00,
  "discount": 0.00,
  "notes": "Updated notes"
}
```

**Headers Required:** `Authorization: Bearer {token}`

---

## Enums Reference

### Gender
- `Male`
- `Female`

### AppointmentStatus
- `Scheduled` - Initial state
- `Confirmed` - Appointment confirmed
- `InProgress` - Currently in progress
- `Completed` - Finished
- `Cancelled` - Cancelled by patient or clinic
- `Rescheduled` - Moved to different date/time
- `NoShow` - Patient didn't show up

### AppointmentType
- `FirstVisit` - First time patient
- `FollowUp` - Follow-up visit
- `Emergency` - Emergency appointment
- `Consultation` - Consultation only

### User Roles
User roles are managed through ASP.NET Core Identity. The application uses role-based authentication for access control.

---

## Error Response Format

All error responses follow this structure:

```json
{
  "statusCode": 400,
  "message": "Error message description",
  "details": "Additional error details (optional)"
}
```

### Common HTTP Status Codes
- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid authentication
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Notes for Frontend Implementation

1. **Authentication Flow:**
   - Call `/auth/login` to get access token
   - Store access token securely (e.g., in memory or secure storage)
   - Include access token in Authorization header for all protected requests
   - Refresh token is stored as HTTP-only cookie automatically
   - Call `/auth/refresh-token` when access token expires (typically 401 response)

2. **Pagination:**
   - Default page size is 10 items
   - Page numbers start from 1
   - Response includes total count and total pages for UI pagination

3. **Date/Time Format:**
   - All dates are in ISO 8601 format (UTC)
   - Example: `2024-02-01T10:00:00Z`

4. **CORS:**
   - API allows all origins in development mode
   - Configure specific origin for production

5. **Validation:**
   - Request validation is handled server-side
   - Detailed error messages are returned in the response

6. **File Uploads:**
   - Avatar and file uploads should use multipart/form-data
   - File paths are returned as strings in responses
