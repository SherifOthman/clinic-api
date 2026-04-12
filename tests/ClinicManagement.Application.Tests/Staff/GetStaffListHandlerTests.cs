using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Features.Staff.Queries;
using ClinicManagement.Application.Tests.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentAssertions;

namespace ClinicManagement.Application.Tests.Staff;

public class GetStaffListHandlerTests
{
    private readonly IUnitOfWork _uow = TestHandlerHelpers.CreateUow();
    private readonly GetStaffListHandler _handler;

    public GetStaffListHandlerTests()
    {
        _handler = new GetStaffListHandler(_uow);
    }

    private async Task SeedStaffAsync(bool isActive = true, Gender gender = Gender.Male)
    {
        var user = new User
        {
            FirstName = "Test", LastName = "User",
            UserName = $"u_{Guid.NewGuid():N}", Email = $"u_{Guid.NewGuid():N}@test.com",
            Gender = gender,
        };
        _uow.UserEntities.Add(user);
        await _uow.Staff.AddAsync(new Domain.Entities.Staff { UserId = user.Id, ClinicId = Guid.NewGuid(), IsActive = isActive });
        await _uow.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllStaff()
    {
        await SeedStaffAsync();
        await SeedStaffAsync();

        var result = await _handler.Handle(new GetStaffListQuery(PageNumber: 1, PageSize: 10), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByActiveStatus()
    {
        await SeedStaffAsync(isActive: true);
        await SeedStaffAsync(isActive: false);

        var result = await _handler.Handle(new GetStaffListQuery(IsActive: true, PageNumber: 1, PageSize: 10), default);

        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoStaff()
    {
        var result = await _handler.Handle(new GetStaffListQuery(PageNumber: 1, PageSize: 10), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(0);
    }
}
