using ClinicManagement.API.Common.Exceptions;

namespace ClinicManagement.API.Common.Extensions;

/// <summary>
/// Extension methods for handling domain exceptions consistently across endpoints
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts domain exceptions to appropriate HTTP results
    /// </summary>
    public static IResult HandleDomainException(this Exception ex)
    {
        return ex switch
        {
            DomainValidationException validationEx =>
                Results.ValidationProblem(
                    validationEx.ValidationErrors.ToDictionary(
                        kvp => kvp.Key, 
                        kvp => kvp.Value.ToArray())),
            
            InvalidBusinessOperationException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_OPERATION" }),
            
            InvalidInvoiceStateException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_STATE" }),
            
            InvalidAppointmentStateException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_STATE" }),
            
            InvalidPaymentStateException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_STATE" }),
            
            InsufficientStockException => 
                Results.BadRequest(new { error = ex.Message, code = "INSUFFICIENT_STOCK" }),
            
            DiscontinuedMedicineException => 
                Results.BadRequest(new { error = ex.Message, code = "MEDICINE_DISCONTINUED" }),
            
            ExpiredMedicineException => 
                Results.BadRequest(new { error = ex.Message, code = "MEDICINE_EXPIRED" }),
            
            InvalidDiscountException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_DISCOUNT" }),
            
            InvalidMoneyException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_MONEY" }),
            
            InvalidEmailException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_EMAIL" }),
            
            InvalidPhoneNumberException => 
                Results.BadRequest(new { error = ex.Message, code = "INVALID_PHONE" }),
            
            DomainException domainEx => 
                Results.BadRequest(new { error = domainEx.Message, code = domainEx.ErrorCode }),
            
            _ => throw ex
        };
    }
}
