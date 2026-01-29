using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientsWithPagination;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Patients.Queries;

public class GetPatientsWithPaginationQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<GetPatientsWithPaginationQueryHandler>> _loggerMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly GetPatientsWithPaginationQueryHandler _handler;

    public GetPatientsWithPaginationQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<GetPatientsWithPaginationQueryHandler>>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        
        _unitOfWorkMock.Setup(x => x.Patients).Returns(_patientRepositoryMock.Object);
        
        _handler = new GetPatientsWithPaginationQueryHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoClinicAccess_ShouldReturnFailure()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(1);
        
        var query = new GetPatientsWithPaginationQuery(1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Code.Should().Be(MessageCodes.Authorization.USER_NO_CLINIC_ACCESS);
    }

    [Fact]
    public async Task Handle_WhenUserHasClinicAccess_ShouldReturnPagedPatients()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(true);
        _currentUserServiceMock.Setup(x => x.ClinicId).Returns(1);
        
        var patients = new List<Patient>
        {
            new Patient { Id = 1, ClinicId = 1, FullName = "John Doe", Gender = Gender.Male },
            new Patient { Id = 2, ClinicId = 1, FullName = "Jane Smith", Gender = Gender.Female }
        };
        
        var pagedResult = new PagedResult<Patient>(patients, 2, 1, 10);
        
        _patientRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);
        
        var query = new GetPatientsWithPaginationQuery(1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.HasClinicAccess()).Returns(true);
        _currentUserServiceMock.Setup(x => x.ClinicId).Returns(1);
        
        var pagedResult = new PagedResult<Patient>(new List<Patient>(), 0, 1, 10);
        
        _patientRepositoryMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);
        
        var query = new GetPatientsWithPaginationQuery(2, 20)
        {
            SearchTerm = "John",
            SortBy = "FullName",
            SortDescending = true,
            Gender = Gender.Male
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        _patientRepositoryMock.Verify(x => x.GetPagedAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}