using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Branches;

public class SetBranchActiveStatusHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IBranchRepository> _branchesMock = new();
    private readonly SetBranchActiveStatusHandler _handler;

    public SetBranchActiveStatusHandlerTests()
    {
        _uowMock.Setup(u => u.Branches).Returns(_branchesMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new SetBranchActiveStatusHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBranchNotFound()
    {
        var id = Guid.NewGuid();
        _branchesMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((ClinicBranch?)null);

        var result = await _handler.Handle(new SetBranchActiveStatusCommand(id, false), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeactivatingMainBranch()
    {
        var branch = TestHandlerHelpers.CreateTestBranch(isMainBranch: true);
        _branchesMock.Setup(x => x.GetByIdAsync(branch.Id, default)).ReturnsAsync(branch);

        var result = await _handler.Handle(new SetBranchActiveStatusCommand(branch.Id, false), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.OPERATION_NOT_ALLOWED);
    }

    [Fact]
    public async Task Handle_ShouldDeactivate_WhenNotMainBranch()
    {
        var branch = TestHandlerHelpers.CreateTestBranch(isMainBranch: false);
        branch.IsActive = true;
        _branchesMock.Setup(x => x.GetByIdAsync(branch.Id, default)).ReturnsAsync(branch);

        var result = await _handler.Handle(new SetBranchActiveStatusCommand(branch.Id, false), default);

        result.IsSuccess.Should().BeTrue();
        branch.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldActivate_WhenBranchIsInactive()
    {
        var branch = TestHandlerHelpers.CreateTestBranch(isMainBranch: false);
        branch.IsActive = false;
        _branchesMock.Setup(x => x.GetByIdAsync(branch.Id, default)).ReturnsAsync(branch);

        var result = await _handler.Handle(new SetBranchActiveStatusCommand(branch.Id, true), default);

        result.IsSuccess.Should().BeTrue();
        branch.IsActive.Should().BeTrue();
    }
}
