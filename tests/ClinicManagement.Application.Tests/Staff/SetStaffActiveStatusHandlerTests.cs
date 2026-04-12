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
        var staff = TestHandlerHelpers.CreateTestStaff();
        staff.IsActive = initial;
        await _uow.Staff.AddAsync(staff);
        await _uow.SaveChangesAsync();

        var result = await _handler.Handle(new SetStaffActiveStatusCommand(staff.Id, target), default);

        result.IsSuccess.Should().BeTrue();

        var updated = await _uow.Staff.GetByIdAsync(staff.Id);
        updated!.IsActive.Should().Be(target);
    }
}
