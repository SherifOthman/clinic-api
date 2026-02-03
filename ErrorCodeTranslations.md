# Error Code Translation System

This document demonstrates how the error code system works for internationalization.

## How It Works

1. **Validators** use error codes from `MessageCodes` class instead of hardcoded English messages
2. **Handlers** return error codes for business logic failures
3. **API** returns error codes in responses
4. **Frontend** translates error codes to user's language

## Example Error Code Usage

### In Validators

```csharp
RuleFor(x => x.Name)
    .NotEmpty().WithMessage(MessageCodes.Medicine.NAME_REQUIRED)
    .MaximumLength(200).WithMessage(MessageCodes.Medicine.NAME_TOO_LONG);
```

### In Handlers

```csharp
if (existingMedicine != null)
{
    return Result<Guid>.Fail(MessageCodes.Medicine.ALREADY_EXISTS);
}
```

### API Response

```json
{
  "success": false,
  "code": "MEDICINE.ALREADY_EXISTS",
  "errors": null
}
```

## Sample Translation Files

### English (en.json)

```json
{
  "MEDICINE.NAME.REQUIRED": "Medicine name is required",
  "MEDICINE.NAME.TOO_LONG": "Medicine name must not exceed 200 characters",
  "MEDICINE.ALREADY_EXISTS": "A medicine with this name already exists",
  "MEDICINE.PRICE.MUST_BE_POSITIVE": "Price must be greater than 0",
  "COMMON.CLINIC_BRANCH.REQUIRED": "Clinic branch is required",
  "COMMON.CLINIC_BRANCH.NOT_FOUND": "Clinic branch not found"
}
```

### Arabic (ar.json)

```json
{
  "MEDICINE.NAME.REQUIRED": "اسم الدواء مطلوب",
  "MEDICINE.NAME.TOO_LONG": "اسم الدواء يجب ألا يتجاوز 200 حرف",
  "MEDICINE.ALREADY_EXISTS": "يوجد دواء بهذا الاسم بالفعل",
  "MEDICINE.PRICE.MUST_BE_POSITIVE": "السعر يجب أن يكون أكبر من 0",
  "COMMON.CLINIC_BRANCH.REQUIRED": "فرع العيادة مطلوب",
  "COMMON.CLINIC_BRANCH.NOT_FOUND": "فرع العيادة غير موجود"
}
```

## Frontend Usage Example (React/TypeScript)

```typescript
// Translation hook
const useTranslation = () => {
  const { language } = useLanguage();

  const translate = (code: string): string => {
    const translations = getTranslations(language);
    return translations[code] || code;
  };

  return { t: translate };
};

// Error handling
const handleApiError = (error: ApiError) => {
  const { t } = useTranslation();

  if (error.errors) {
    // Field-specific errors
    error.errors.forEach((fieldError) => {
      setFieldError(fieldError.field, t(fieldError.code));
    });
  } else {
    // General error
    showToast(t(error.code), "error");
  }
};
```

## Benefits

1. **Consistent Error Messages**: All error messages are centralized
2. **Easy Translation**: Frontend can translate any error code to any language
3. **Maintainable**: Changes to error messages only need to be made in translation files
4. **Type Safety**: Error codes are constants, reducing typos
5. **Flexible**: Same error code can have different translations based on context

## Error Code Categories

- **AUTH**: Authentication related errors
- **AUTHZ**: Authorization related errors
- **VALIDATION**: Input validation errors
- **BUSINESS**: Business logic errors
- **MEDICINE**: Medicine-specific errors
- **MEDICAL_SUPPLY**: Medical supply-specific errors
- **MEDICAL_SERVICE**: Medical service-specific errors
- **INVOICE**: Invoice-specific errors
- **PAYMENT**: Payment-specific errors
- **MEASUREMENT**: Measurement-specific errors
- **APPOINTMENT**: Appointment-specific errors
- **CHRONIC_DISEASE**: Chronic disease-specific errors
- **COMMON**: Common errors used across features
- **FIELDS**: Field validation errors
- **EXCEPTION**: System exception errors
