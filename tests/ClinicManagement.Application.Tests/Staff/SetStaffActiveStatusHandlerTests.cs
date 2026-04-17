using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Staff;

public class SetStaffActiveStatusHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly SetStaffActiveStatusHandler _handler;

    public SetStaffActiveStatusHandlerTests()
    {
        _handler = new SetStaffActiveStatusHandler(_uow);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStaffNotFound()
    {
        var result = await _handler.Handle(new SetStaffActiveStatusCommand(Guid.NewGuid(), false), default);
        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_ShouldToggleActiveStatus(bool initial, bool target)
    {
        var (_, member) = TestHandlerHelpers.CreateTestMember();
        member.IsActive = initial;
        await _uow.Members.AddAsync(member);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new SetStaffActiveStatusCommand(member.Id, target), default);

        result.IsSuccess.Should().BeTrue();

        var updated = await _uow.Members.GetByIdAsync(member.Id);
        updated!.IsActive.Should().Be(target);
    }
}
