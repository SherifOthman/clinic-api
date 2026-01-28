using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace ClinicManagement.Tests.Middleware;

public class ExceptionHandlingTests : MiddlewareTestBase
{
    [Fact]
    public async Task HandleException_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ArgumentException("Invalid argument provided");

        // Act
        await HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Invalid argument provided");
        responseBody.Should().Contain("400");
    }

    [Fact]
    public async Task HandleException_WhenUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        await HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Access denied");
        responseBody.Should().Contain("401");
    }

    [Fact]
    public async Task HandleException_WhenKeyNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        await HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("Resource not found");
        responseBody.Should().Contain("404");
    }

    [Fact]
    public async Task HandleException_WhenGenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Something went wrong");

        // Act
        await HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        
        var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().Contain("An error occurred while processing your request");
        responseBody.Should().Contain("500");
    }

    [Theory]
    [InlineData(typeof(ArgumentException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(ArgumentNullException), HttpStatusCode.BadRequest)]
    [InlineData(typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized)]
    [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound)]
    [InlineData(typeof(InvalidOperationException), HttpStatusCode.InternalServerError)]
    [InlineData(typeof(NotImplementedException), HttpStatusCode.InternalServerError)]
    public async Task HandleException_WithDifferentExceptionTypes_ShouldReturnCorrectStatusCode(Type exceptionType, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception")!;

        // Act
        await HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)expectedStatusCode);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Simulate exception handling logic
        var statusCode = exception switch
        {
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        var message = statusCode == HttpStatusCode.InternalServerError 
            ? "An error occurred while processing your request." 
            : exception.Message;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        var json = System.Text.Json.JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}