# API Contracts Refactoring

## Date

February 28, 2026

## Problem

Commands (MediatR messages) were being exposed in Swagger/Scalar API documentation, which is incorrect because:

- Commands are internal application messages, not API contracts
- This creates coupling between API and Application layer
- API documentation shows implementation details instead of clean contracts

## Solution

### Architecture Decision

1. **Request DTOs** - Separate API contracts in `API/Contracts` folder
2. **Commands** - Stay internal to Application layer (not exposed)
3. **Response DTOs** - Reuse Application layer DTOs directly (no duplication)

### Implementation

#### Created Request DTOs (API Layer)

```
src/ClinicManagement.API/Contracts/
├── Auth/
│   └── AuthRequests.cs (LoginRequest, RegisterRequest, etc.)
├── Staff/
│   └── StaffRequests.cs (InviteStaffRequest, etc.)
└── Onboarding/
    └── OnboardingRequests.cs (CompleteOnboardingRequest)
```

#### Controller Pattern

```csharp
// Before (Command exposed in API)
public async Task<IActionResult> Register([FromBody] RegisterCommand command)
{
    var result = await Sender.Send(command);
    return HandleResult(result);
}

// After (Request DTO mapped to Command)
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    var command = new RegisterCommand(
        request.FirstName,
        request.LastName,
        request.UserName,
        request.Email,
        request.Password,
        request.PhoneNumber
    );
    var result = await Sender.Send(command);
    return HandleResult(result);
}
```

#### Response Pattern

```csharp
// Use Application DTOs directly (no duplication)
[ProducesResponseType(typeof(GetMeDto), StatusCodes.Status200OK)]
public async Task<IActionResult> GetMe()
{
    var result = await Sender.Send(query);
    return Ok(result); // Return Application DTO directly
}
```

## Benefits

1. **Clean API Documentation** - Only API contracts visible in Swagger/Scalar
2. **No Duplication** - Response DTOs not duplicated between layers
3. **Proper Separation** - API contracts separate from internal messages
4. **Single Source of Truth** - Response shapes defined once in Application layer
5. **Flexibility** - Can change internal commands without breaking API contracts

## Files Changed

- Created: 3 request DTO files
- Modified: 3 controllers (Auth, Staff, Onboarding)
- Deleted: 2 response DTO files (duplicates removed)

## Testing

- Build: ✅ Successful (1 pre-existing warning)
- Tests: ✅ All 32 passing

## Commit

`af0e70f` - refactor: separate API request DTOs from commands, use Application DTOs for responses
