# Value Objects - Step 3 of DDD Improvements

## What Are Value Objects?

**Value Objects** are immutable objects that represent a descriptive aspect of the domain with no conceptual identity. They are defined by their attributes, not by an ID.

### Key Characteristics:

1. **Immutable** - Once created, cannot be changed
2. **No Identity** - Two value objects with same values are equal
3. **Self-Validating** - Validation happens in constructor
4. **Encapsulate Behavior** - Contains related logic

### Examples:

- `Email` - Not just a string, has validation rules
- `Money` - Amount + Currency, has arithmetic operations
- `PhoneNumber` - Format, country code, validation
- `Address` - Street, city, postal code as one concept
- `DateRange` - Start and end date with validation

## Why Use Value Objects?

### Problem: Primitive Obsession

**BEFORE (Primitive Obsession):**

```csharp
public class Patient
{
    public string Email { get; set; } = null!; // Just a string!
    public decimal InvoiceAmount { get; set; } // What currency?
    public string PhoneNumber { get; set; } = null!; // Any format?
}

// Validation scattered everywhere
if (!email.Contains("@")) throw new Exception("Invalid email");
if (amount < 0) throw new Exception("Amount must be positive");
if (!phoneNumber.StartsWith("+")) throw new Exception("Invalid phone");
```

**Problems:**

- Validation duplicated across the codebase
- No type safety (can pass any string)
- Business rules scattered
- Hard to test
- Easy to make mistakes

**AFTER (Value Objects):**

```csharp
public class Patient
{
    public Email Email { get; private set; } = null!;
    public Money InvoiceAmount { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
}

// Validation happens once in constructor
var email = new Email("john@example.com"); // ✅ Valid
var email = new Email("invalid"); // ❌ Throws DomainException

// Type safety
patient.SetEmail(new Email("test@example.com")); // ✅ Correct
patient.SetEmail("test@example.com"); // ❌ Won't compile
```

### Benefits:

#### 1. **Type Safety**

```csharp
// Before: Easy to mix up
void SendInvoice(string email, string phoneNumber) { }
SendInvoice(phoneNumber, email); // ❌ Compiles but wrong!

// After: Compiler catches errors
void SendInvoice(Email email, PhoneNumber phone) { }
SendInvoice(phone, email); // ❌ Won't compile!
```

#### 2. **Single Source of Truth**

```csharp
// Validation in ONE place
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailException("Email cannot be empty");
        if (!value.Contains("@"))
            throw new InvalidEmailException("Email must contain @");
        if (value.Length > 255)
            throw new InvalidEmailException("Email too long");

        Value = value.ToLowerInvariant();
    }
}
```

#### 3. **Encapsulate Behavior**

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public bool IsPositive => Amount > 0;
    public bool IsZero => Amount == 0;
}
```

#### 4. **Equality by Value**

```csharp
var email1 = new Email("john@example.com");
var email2 = new Email("john@example.com");

email1 == email2; // ✅ True (same value)
email1.Equals(email2); // ✅ True

// Entities are different
var patient1 = new Patient { Id = Guid.NewGuid() };
var patient2 = new Patient { Id = Guid.NewGuid() };
patient1 == patient2; // ❌ False (different IDs)
```

## Value Objects vs Entities

| Aspect     | Entity           | Value Object |
| ---------- | ---------------- | ------------ |
| Identity   | Has unique ID    | No identity  |
| Equality   | By ID            | By value     |
| Mutability | Mutable          | Immutable    |
| Lifespan   | Has lifecycle    | Replaceable  |
| Example    | Patient, Invoice | Email, Money |

```csharp
// Entity - has identity
var patient1 = new Patient { Id = Guid.NewGuid(), Name = "John" };
var patient2 = new Patient { Id = Guid.NewGuid(), Name = "John" };
patient1 == patient2; // False - different patients

// Value Object - no identity
var email1 = new Email("john@example.com");
var email2 = new Email("john@example.com");
email1 == email2; // True - same email
```

## Implementation Strategy

We'll create these value objects:

1. **Email** - Email validation and normalization
2. **Money** - Amount with currency
3. **PhoneNumber** - Phone number with country code
4. **Address** (optional) - Complete address as one concept

### Base ValueObject Class

```csharp
public abstract class ValueObject
{
    // Equality by comparing all properties
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
```

## EF Core Configuration

Value objects need special configuration in EF Core:

```csharp
// Option 1: Owned Entity (Recommended)
builder.OwnsOne(p => p.Email, email =>
{
    email.Property(e => e.Value)
        .HasColumnName("Email")
        .HasMaxLength(255)
        .IsRequired();
});

// Option 2: Value Conversion
builder.Property(p => p.Email)
    .HasConversion(
        email => email.Value,
        value => new Email(value)
    );
```

## Industry Standards

This implementation follows:

1. **Domain-Driven Design (Eric Evans)**
   - Value objects are fundamental building blocks
   - Immutable and self-validating

2. **Clean Architecture (Robert C. Martin)**
   - Domain concepts encapsulated
   - No primitive obsession

3. **Microsoft eShopOnContainers**
   - Uses value objects for Money, Address
   - Configured as owned entities

4. **Vladimir Khorikov (DDD in Practice)**
   - Value objects for all domain concepts
   - Validation in constructor

## Next Steps

After implementing value objects:

1. Update entities to use value objects
2. Configure EF Core mappings
3. Update DTOs and mappings
4. Update tests
5. Verify API still works

Let's start implementing!
