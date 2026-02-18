# Folder Structure Guidelines

## Overview

This document describes the consistent folder structure pattern used throughout the application.

## Pattern Rules

### Commands

Commands are organized based on complexity:

**Multi-file Commands (kept in folders):**

- Commands with validators stay in their own folders
- Structure: `Commands/{CommandName}/{CommandName}.cs` + `{CommandName}Validator.cs`
- Example: `Commands/Login/Login.cs` + `LoginValidator.cs`

**Single-file Commands (flattened):**

- Commands without validators are flattened to the Commands folder
- Structure: `Commands/{CommandName}.cs`
- Example: `Commands/Logout.cs`

### Queries

All queries are flattened to single files:

- Structure: `Queries/{QueryName}.cs`
- Each file contains: Query record + Dto record + Handler class
- Example: `Queries/GetMe.cs`

## Current Structure

### Auth Commands

```
Features/Auth/Commands/
├── ChangePassword/
│   ├── ChangePassword.cs (Command + Handler)
│   └── ChangePasswordValidator.cs
├── ConfirmEmail/
│   ├── ConfirmEmail.cs (Command + Handler)
│   └── ConfirmEmailValidator.cs
├── Login/
│   ├── Login.cs (Command + Handler)
│   └── LoginValidator.cs
├── Register/
│   ├── Register.cs (Command + Handler)
│   └── RegisterValidator.cs
├── DeleteProfileImage.cs (Command + Handler)
├── ForgotPassword.cs (Command + Handler + Validator)
├── Logout.cs (Command + Handler)
├── RefreshToken.cs (Command + Handler)
├── ResendEmailVerification.cs (Command + Handler + Validator)
├── ResetPassword.cs (Command + Handler + Validator)
├── UpdateProfile.cs (Command + Handler + Validator)
└── UploadProfileImage.cs (Command + Handler)
```

### Auth Queries

```
Features/Auth/Queries/
├── CheckEmailAvailability.cs (Query + Dto + Handler)
├── CheckUsernameAvailability.cs (Query + Dto + Handler)
└── GetMe.cs (Query + Dto + Handler)
```

### SubscriptionPlans Queries

```
Features/SubscriptionPlans/Queries/
└── GetSubscriptionPlans.cs (Query + Dto + Handler)
```

## File Content Pattern

### Command with Validator (in folder)

```csharp
// Commands/Login/Login.cs
namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public record LoginCommand(...) : IRequest<Result<LoginResponseDto>>;
public record LoginResponseDto(...);
public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>> { }

// Commands/Login/LoginValidator.cs
namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public class LoginValidator : AbstractValidator<LoginCommand> { }
```

### Command without Validator (flattened)

```csharp
// Commands/Logout.cs
namespace ClinicManagement.Application.Features.Auth.Commands;

public record LogoutCommand(...) : IRequest<Result>;
public class LogoutHandler : IRequestHandler<LogoutCommand, Result> { }
```

### Query (always flattened)

```csharp
// Queries/GetMe.cs
namespace ClinicManagement.Application.Features.Auth.Queries;

public record GetMeQuery(...) : IRequest<GetMeDto?>;
public record GetMeDto(...);
public class GetMeHandler : IRequestHandler<GetMeQuery, GetMeDto?> { }
```

## Benefits

1. **Consistency**: Same pattern across all features
2. **Discoverability**: Easy to find related code
3. **Simplicity**: Minimal folder nesting
4. **Clarity**: Folder structure indicates complexity
5. **Maintainability**: Easy to add new features following the pattern

## Adding New Features

### New Command with Validator

1. Create folder: `Commands/{CommandName}/`
2. Create file: `{CommandName}.cs` (Command + Handler)
3. Create file: `{CommandName}Validator.cs`
4. Use namespace: `...Commands.{CommandName}`

### New Command without Validator

1. Create file: `Commands/{CommandName}.cs` (Command + Handler)
2. Use namespace: `...Commands`

### New Query

1. Create file: `Queries/{QueryName}.cs` (Query + Dto + Handler)
2. Use namespace: `...Queries`

## Migration Notes

This structure was established after migrating from:

- EF Core + Identity → Dapper + Repository/UnitOfWork
- Custom result types → Result pattern
- Nested folders → Flattened structure
- Separate files → Merged files

All changes maintain Clean Architecture principles with proper layer separation.
