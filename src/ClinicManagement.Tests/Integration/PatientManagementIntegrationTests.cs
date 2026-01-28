using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Application.Features.Patients.Commands.DeletePatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientsWithPagination;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ClinicManagement.Tests.Integration;

public class PatientManagementIntegrationTests : ApiTestBase
{
    [Fact]
    public async Task CreatePatient_WhenValidData_ShouldCreatePatientSuccessfully()
    {
        // Arrange
        var clinic = await CreateTestClinicAsync();
        var user = await CreateTestUserAsync("doctor@test.com", "Password123!", "Doctor", clinic.Id);
        
        SetCurrentUser(user.Id, clinic.Id);

        var command = new CreatePatientCommand
        {
            FullName = "John Doe",
            DateOfBirth = DateTime.Today.AddYears(-30),
            Gender = Gender.Male,
            Address = "123 Main Street",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214", Label = "Mobile" }
            },
            ChronicDiseaseIds = new List<int>()
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FullName.Should().Be("John Doe");
        result.Value.Gender.Should().Be("Male");

        // Verify patient was created in database
        var patient = await _context.Patients.FindAsync(result.Value.Id);
        patient.Should().NotBeNull();
        patient!.FullName.Should().Be("John Doe");
        patient.ClinicId.Should().Be(clinic.Id);
    }

    [Fact]
    public async Task UpdatePatient_WhenValidData_ShouldUpdatePatientSuccessfully()
    {
        // Arrange
        var clinic = await CreateTestClinicAsync();
        var user = await CreateTestUserAsync("doctor@test.com", "Password123!", "Doctor", clinic.Id);
        var patient = await CreateTestPatientAsync("Jane Doe", clinic.Id);
        
        SetCurrentUser(user.Id, clinic.Id);

        var command = new UpdatePatientCommand
        {
            Id = patient.Id,
            FullName = "Jane Smith", // Updated name
            DateOfBirth = DateTime.Today.AddYears(-25),
            Gender = Gender.Female,
            Address = "456 Oak Avenue",
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021215", Label = "Mobile" }
            },
            ChronicDiseaseIds = new List<int>()
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FullName.Should().Be("Jane Smith");

        // Verify patient was updated in database
        var updatedPatient = await _context.Patients.FindAsync(patient.Id);
        updatedPatient.Should().NotBeNull();
        updatedPatient!.FullName.Should().Be("Jane Smith");
        updatedPatient.Address.Should().Be("456 Oak Avenue");
    }

    [Fact]
    public async Task DeletePatient_WhenValidId_ShouldDeletePatientSuccessfully()
    {
        // Arrange
        var clinic = await CreateTestClinicAsync();
        var user = await CreateTestUserAsync("doctor@test.com", "Password123!", "Doctor", clinic.Id);
        var patient = await CreateTestPatientAsync("John Doe", clinic.Id);
        
        SetCurrentUser(user.Id, clinic.Id);

        var command = new DeletePatientCommand { Id = patient.Id };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeTrue();

        // Verify patient was deleted from database
        var deletedPatient = await _context.Patients.FindAsync(patient.Id);
        deletedPatient.Should().BeNull();
    }

    [Fact]
    public async Task GetPatientsWithPagination_WhenPatientsExist_ShouldReturnPagedResults()
    {
        // Arrange
        var clinic = await CreateTestClinicAsync();
        var user = await CreateTestUserAsync("doctor@test.com", "Password123!", "Doctor", clinic.Id);
        
        // Create multiple patients
        await CreateTestPatientAsync("Patient 1", clinic.Id);
        await CreateTestPatientAsync("Patient 2", clinic.Id);
        await CreateTestPatientAsync("Patient 3", clinic.Id);
        
        SetCurrentUser(user.Id, clinic.Id);

        var query = new GetPatientsWithPaginationQuery
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task CreatePatient_WhenUserFromDifferentClinic_ShouldFail()
    {
        // Arrange
        var clinic1 = await CreateTestClinicAsync();
        var clinic2 = await CreateTestClinicAsync("Another Clinic");
        var user = await CreateTestUserAsync("doctor@test.com", "Password123!", "Doctor", clinic1.Id);
        
        SetCurrentUser(user.Id, clinic2.Id); // Different clinic

        var command = new CreatePatientCommand
        {
            FullName = "John Doe",
            PhoneNumbers = new List<CreatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021214" }
            }
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePatient_WhenPatientNotInUserClinic_ShouldFail()
    {
        // Arrange
        var clinic1 = await CreateTestClinicAsync();
        var clinic2 = await CreateTestClinicAsync("Another Clinic");
        var user = await CreateTestUserAsync("doctor@test.com", "Password123!", "Doctor", clinic1.Id);
        var patient = await CreateTestPatientAsync("Jane Doe", clinic2.Id); // Different clinic
        
        SetCurrentUser(user.Id, clinic1.Id);

        var command = new UpdatePatientCommand
        {
            Id = patient.Id,
            FullName = "Jane Smith",
            PhoneNumbers = new List<UpdatePatientPhoneNumberDto>
            {
                new() { PhoneNumber = "+201098021215" }
            }
        };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Success.Should().BeFalse();
    }

    private async Task<Patient> CreateTestPatientAsync(string fullName, int clinicId)
    {
        var patient = new Patient
        {
            FullName = fullName,
            ClinicId = clinicId,
            DateOfBirth = DateTime.Today.AddYears(-30),
            Gender = Gender.Male,
            CreatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    private async Task<Clinic> CreateTestClinicAsync(string name = "Test Clinic")
    {
        var subscriptionPlan = new SubscriptionPlan
        {
            Name = "Basic Plan",
            Price = 29.99m,
            IsActive = true
        };

        _context.SubscriptionPlans.Add(subscriptionPlan);
        await _context.SaveChangesAsync();

        var clinic = new Clinic
        {
            Name = name,
            SubscriptionPlanId = subscriptionPlan.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();
        return clinic;
    }
}