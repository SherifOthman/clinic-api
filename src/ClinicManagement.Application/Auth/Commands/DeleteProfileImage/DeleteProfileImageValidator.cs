using FluentValidation;

namespace ClinicManagement.Application.Auth.Commands;

public class DeleteProfileImageValidator : AbstractValidator<DeleteProfileImageCommand>
{
    public DeleteProfileImageValidator()
    {
        // No validation rules needed - command has no properties
        // This validator exists for consistency and future extensibility
    }
}
