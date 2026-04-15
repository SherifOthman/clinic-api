using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Application.Tests.Common;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Branches;

public class CreateBranchHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CreateBranchHandler _handler;

    public CreateBranchHandlerTests()
    {
        _handler = new CreateBranchHandler(_uow, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateBranch_WithCorrectClinicAndDefaults()
    {
        var clinicId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(clinicId);

        var result = await _handler.Handle(
            new CreateBranchCommand("North Branch", "123 North St", 1, 2, []), default);

        result.IsSuccess.Should().BeTrue();

        var branch = await _uow.Branches.GetByIdAsync(result.Value);
        branch!.Name.Should().Be("North Branch");
        branch.ClinicId.Should().Be(clinicId);
        branch.IsMainBranch.Should().BeFalse();
        branch.IsActive.Should().BeTrue();
    }
}
