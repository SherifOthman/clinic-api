using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Branches;

public class CreateBranchHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IBranchRepository> _branchesMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly CreateBranchHandler _handler;

    public CreateBranchHandlerTests()
    {
        _uowMock.Setup(u => u.Branches).Returns(_branchesMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new CreateBranchHandler(_uowMock.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateBranch_WithCorrectClinicAndDefaults()
    {
        var clinicId = Guid.NewGuid();
        _currentUserMock.Setup(x => x.GetRequiredClinicId()).Returns(clinicId);

        ClinicBranch? captured = null;
        _branchesMock
            .Setup(x => x.AddAsync(It.IsAny<ClinicBranch>(), default))
            .Callback<ClinicBranch, CancellationToken>((b, _) => captured = b)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new CreateBranchCommand("North Branch", "123 North St", 1, 2, []), default);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("North Branch");
        captured.ClinicId.Should().Be(clinicId);
        captured.IsMainBranch.Should().BeFalse();
        captured.IsActive.Should().BeTrue();
    }
}
