using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Staff.Commands;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace ClinicManagement.Application.Tests.Staff;

public class SetStaffActiveStatusHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IClinicMemberRepository> _membersMock = new();
    private readonly SetStaffActiveStatusHandler _handler;

    public SetStaffActiveStatusHandlerTests()
    {
        _uowMock.Setup(u => u.Members).Returns(_membersMock.Object);
        _uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        _handler = new SetStaffActiveStatusHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStaffNotFound()
    {
        var id = Guid.NewGuid();
        _membersMock.Setup(x => x.GetByIdAsync(id, default)).ReturnsAsync((ClinicMember?)null);

        var result = await _handler.Handle(new SetStaffActiveStatusCommand(id, false), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Handle_ShouldToggleActiveStatus(bool initial, bool target)
    {
        var member = TestHandlerHelpers.CreateTestMember();
        member.IsActive = initial;
        _membersMock.Setup(x => x.GetByIdAsync(member.Id, default)).ReturnsAsync(member);

        var result = await _handler.Handle(new SetStaffActiveStatusCommand(member.Id, target), default);

        result.IsSuccess.Should().BeTrue();
        member.IsActive.Should().Be(target);
    }
}
