# PowerShell script to convert FluentValidation to .NET 10 DataAnnotations
# This script processes all endpoint files and converts validators

$files = Get-ChildItem -Path "ClinicManagement.API/Features" -Recurse -Filter "*.cs" | 
    Where-Object { (Get-Content $_.FullName -Raw) -match '\.AddValidationFilter' }

Write-Host "Converting $($files.Count) files from FluentValidation to DataAnnotations..." -ForegroundColor Cyan

foreach ($file in $files) {
    Write-Host "`nProcessing: $($file.Name)" -ForegroundColor Yellow
    
    $content = Get-Content $file.FullName -Raw
    
    # Remove .AddValidationFilter<Request>()
    $content = $content -replace '\.AddValidationFilter<Request>\(\);', ';'
    
    # Remove FluentValidation using
    $content = $content -replace 'using FluentValidation;[\r\n]+', ''
    
    # Remove Common.Extensions using if it exists
    $content = $content -replace 'using ClinicManagement\.API\.Common\.Extensions;[\r\n]+', ''
    
    # Add CustomValidation using if needed (for date validations)
    if ($content -match 'LessThan\(DateTime\.UtcNow\)|GreaterThan\(DateTime\.UtcNow\)') {
        if ($content -notmatch 'using ClinicManagement\.API\.Common\.Validation;') {
            $content = $content -replace '(using ClinicManagement\.API\.Common;)', "`$1`r`nusing ClinicManagement.API.Common.Validation;"
        }
    }
    
    # Remove entire Validator class (from "public class Validator" to the closing brace)
    $content = $content -replace '(?s)public class Validator : AbstractValidator<Request>.*?^\s*\}[\r\n]+', '', 'Multiline'
    
    # Save the file
    Set-Content -Path $file.FullName -Value $content -NoNewline
    
    Write-Host "  - Removed .AddValidationFilter" -ForegroundColor Green
    Write-Host "  - Removed FluentValidation using" -ForegroundColor Green
    Write-Host "  - Removed Validator class" -ForegroundColor Green
}

Write-Host "`nConversion complete! $($files.Count) files processed." -ForegroundColor Green
Write-Host "`nIMPORTANT: You still need to manually add DataAnnotations attributes to Request records!" -ForegroundColor Yellow
Write-Host "See VALIDATION_MIGRATION_GUIDE.md for the conversion patterns." -ForegroundColor Yellow
