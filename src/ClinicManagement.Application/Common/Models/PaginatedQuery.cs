namespace ClinicManagement.Application.Common.Models;

public abstract record PaginatedQuery(int PageNumber = 1, int PageSize = 10);
