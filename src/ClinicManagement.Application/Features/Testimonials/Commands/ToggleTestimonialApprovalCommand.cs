using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public record ToggleTestimonialApprovalCommand(Guid Id) : IRequest<Result>;
