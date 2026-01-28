using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Locations.Queries.GetLocationHierarchy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicManagement.Tests.Application.Features.Locations.Queries;

public class GetLocationHierarchyQueryHandlerTests
{
    private readonly Mock<IGeoNamesClient> _geoNamesClientMock;
    private readonly Mock<ILocationsService> _locationsServiceMock;
    private readonly Mock<ILogger<GetLocationHierarchyQueryHandler>> _loggerMock;
    private readonly GetLocationHierarchyQueryHandler _handler;

    public GetLocationHierarchyQueryHandlerTests()
    {
        _geoNamesClientMock = new Mock<IGeoNamesClient>();
        _locationsServiceMock = new Mock<ILocationsService>();
        _loggerMock = new Mock<ILogger<GetLocationHierarchyQueryHandler>>();

        _handler = new GetLocationHierarchyQueryHandler(
            _geoNamesClientMock.Object,
            _locationsServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenLocationExists_ShouldReturnHierarchy()
    {
        // Arrange
        var query = new GetLocationHierarchyQuery(360630); // Cairo, Egypt

        var locationDetails = new GeoNamesLocationDto
        {
            GeoNameId = 360630,
            Name = "Cairo",
            CountryCode = "EG",
            AdminName1 = "Cairo Governorate"
        };

        var country = new CountryDto { Id = 1, Name = "Egypt", Code = "EG" };
        var states = new List<StateDto>
        {
            new StateDto { Id = 1, Name = "Cairo Governorate", CountryId = 1 }
        };
        var cities = new List<CityDto>
        {
            new CityDto { Id = 360630, Name = "Cairo", CountryId = 1, StateId = 1 }
        };

        _geoNamesClientMock.Setup(x => x.GetLocationByIdAsync(360630))
            .ReturnsAsync(locationDetails);

        _locationsServiceMock.Setup(x => x.GetCountryByCodeAsync("EG"))
            .ReturnsAsync(country);

        _locationsServiceMock.Setup(x => x.GetStatesAsync(1))
            .ReturnsAsync(states);

        _locationsServiceMock.Setup(x => x.GetCitiesAsync(1, 1))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LocationName.Should().Be("Cairo");
        result.Value.Country.Should().NotBeNull();
        result.Value.Country!.Name.Should().Be("Egypt");
        result.Value.State.Should().NotBeNull();
        result.Value.State!.Name.Should().Be("Cairo Governorate");
        result.Value.City.Should().NotBeNull();
        result.Value.City!.Name.Should().Be("Cairo");
    }

    [Fact]
    public async Task Handle_WhenLocationNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetLocationHierarchyQuery(999999);

        _geoNamesClientMock.Setup(x => x.GetLocationByIdAsync(999999))
            .ReturnsAsync((GeoNamesLocationDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Location not found");
    }

    [Fact]
    public async Task Handle_WhenCountryNotFound_ShouldReturnPartialHierarchy()
    {
        // Arrange
        var query = new GetLocationHierarchyQuery(123456);

        var locationDetails = new GeoNamesLocationDto
        {
            GeoNameId = 123456,
            Name = "Unknown City",
            CountryCode = "XX"
        };

        _geoNamesClientMock.Setup(x => x.GetLocationByIdAsync(123456))
            .ReturnsAsync(locationDetails);

        _locationsServiceMock.Setup(x => x.GetCountryByCodeAsync("XX"))
            .ReturnsAsync((CountryDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LocationName.Should().Be("Unknown City");
        result.Value.Country.Should().BeNull();
        result.Value.State.Should().BeNull();
        result.Value.City.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenStateNotFound_ShouldReturnCountryOnly()
    {
        // Arrange
        var query = new GetLocationHierarchyQuery(123457);

        var locationDetails = new GeoNamesLocationDto
        {
            GeoNameId = 123457,
            Name = "Some City",
            CountryCode = "EG",
            AdminName1 = "Unknown Governorate"
        };

        var country = new CountryDto { Id = 1, Name = "Egypt", Code = "EG" };
        var states = new List<StateDto>(); // Empty list - no matching state

        _geoNamesClientMock.Setup(x => x.GetLocationByIdAsync(123457))
            .ReturnsAsync(locationDetails);

        _locationsServiceMock.Setup(x => x.GetCountryByCodeAsync("EG"))
            .ReturnsAsync(country);

        _locationsServiceMock.Setup(x => x.GetStatesAsync(1))
            .ReturnsAsync(states);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LocationName.Should().Be("Some City");
        result.Value.Country.Should().NotBeNull();
        result.Value.Country!.Name.Should().Be("Egypt");
        result.Value.State.Should().BeNull();
        result.Value.City.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetLocationHierarchyQuery(360630);

        _geoNamesClientMock.Setup(x => x.GetLocationByIdAsync(360630))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to retrieve location hierarchy");
    }

    [Fact]
    public async Task Handle_WithoutAdminName_ShouldReturnCountryOnly()
    {
        // Arrange
        var query = new GetLocationHierarchyQuery(123458);

        var locationDetails = new GeoNamesLocationDto
        {
            GeoNameId = 123458,
            Name = "Country Capital",
            CountryCode = "EG",
            AdminName1 = null // No admin name
        };

        var country = new CountryDto { Id = 1, Name = "Egypt", Code = "EG" };

        _geoNamesClientMock.Setup(x => x.GetLocationByIdAsync(123458))
            .ReturnsAsync(locationDetails);

        _locationsServiceMock.Setup(x => x.GetCountryByCodeAsync("EG"))
            .ReturnsAsync(country);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.LocationName.Should().Be("Country Capital");
        result.Value.Country.Should().NotBeNull();
        result.Value.Country!.Name.Should().Be("Egypt");
        result.Value.State.Should().BeNull();
        result.Value.City.Should().BeNull();
    }
}