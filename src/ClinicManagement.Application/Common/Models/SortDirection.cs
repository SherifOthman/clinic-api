namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// Strongly-typed sort direction. Replaces raw "asc"/"desc" strings in filter records.
/// Eliminates silent fallback on invalid string values.
/// </summary>
public enum SortDirection { Asc, Desc }
