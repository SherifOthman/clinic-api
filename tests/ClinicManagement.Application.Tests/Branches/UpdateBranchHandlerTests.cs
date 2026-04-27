using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Application.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Branches;

/// <summary>
/// Unit tests for UpdateBranchHandler.
/// Phone number replacement (Clear + AddRange) requires a real DB due to EF in-memory
/// provider limitations — those scenarios are covered in BranchesEndpointsTests.
/// </summary>
public class UpdateBranchHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly UpdateBranchHandler _handler;

    public UpdateBranchHandlerTests()
    {
        _handler = new UpdateBranchHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBranchNotFound()
    {
        var result = await _handler.Handle(
            new UpdateBranchCommand(Guid.NewGuid(), "New Name", "New Address", null, null, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenBranchExists()
    {
        var branch = TestHandlerHelpers.CreateTestBranch();
        await _uow.Branches.AddAsync(branch);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(
            new UpdateBranchCommand(branch.Id, "Updated Name", "Updated Address", null, null, []), default);

        result.IsSuccess.Should().BeTrue();
        branch.Name.Should().Be("Updated Name");
        branch.AddressLine.Should().Be("Updated Address");
    }
}
