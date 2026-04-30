using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Common.Constants;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Branches;

public class SetBranchActiveStatusHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly SetBranchActiveStatusHandler _handler;

    public SetBranchActiveStatusHandlerTests()
    {
        _handler = new SetBranchActiveStatusHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBranchNotFound()
    {
        var result = await _handler.Handle(
            new SetBranchActiveStatusCommand(Guid.NewGuid(), false), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.NOT_FOUND);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeactivatingMainBranch()
    {
        var branch = TestHandlerHelpers.CreateTestBranch(isMainBranch: true);
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new SetBranchActiveStatusCommand(branch.Id, false), default);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(ErrorCodes.OPERATION_NOT_ALLOWED);
    }

    [Fact]
    public async Task Handle_ShouldDeactivate_WhenNotMainBranch()
    {
        var branch = TestHandlerHelpers.CreateTestBranch(isMainBranch: false);
        branch.IsActive = true;
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new SetBranchActiveStatusCommand(branch.Id, false), default);

        result.IsSuccess.Should().BeTrue();
        branch.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldActivate_WhenBranchIsInactive()
    {
        var branch = TestHandlerHelpers.CreateTestBranch(isMainBranch: false);
        branch.IsActive = false;
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new SetBranchActiveStatusCommand(branch.Id, true), default);

        result.IsSuccess.Should().BeTrue();
        branch.IsActive.Should().BeTrue();
    }
}
