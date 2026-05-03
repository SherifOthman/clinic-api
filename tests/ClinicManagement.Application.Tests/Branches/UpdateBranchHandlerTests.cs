using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Branches.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Branches;

public class UpdateBranchHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IBranchRepository> _branchesMock = new();
    private readonly UpdateBranchHandler _handler;

    public UpdateBranchHandlerTests()
    {
        _uowMock.Setup(u => u.Branches).Returns(_branchesMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new UpdateBranchHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBranchNotFound()
    {
        var id = Guid.NewGuid();
        _branchesMock.Setup(x => x.GetByIdWithPhonesAsync(id, default)).ReturnsAsync((ClinicBranch?)null);

        var result = await _handler.Handle(
            new UpdateBranchCommand(id, "New Name", "New Address", null, null, []), default);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenBranchExists()
    {
        var branch = TestHandlerHelpers.CreateTestBranch();
        _branchesMock.Setup(x => x.GetByIdWithPhonesAsync(branch.Id, default)).ReturnsAsync(branch);

        var result = await _handler.Handle(
            new UpdateBranchCommand(branch.Id, "Updated Name", "Updated Address", null, null, []), default);

        result.IsSuccess.Should().BeTrue();
        branch.Name.Should().Be("Updated Name");
        branch.AddressLine.Should().Be("Updated Address");
    }
}
