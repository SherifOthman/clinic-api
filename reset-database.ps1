# Database Reset Script
Write-Host "=== Database Reset and Reseed Script ===" -ForegroundColor Cyan

$ServerInstance = "localhost"
$Database = "ClinicManagementDb"
$SqlScriptPath = "reset-and-reseed.sql"

Write-Host "Checking SQL Server connection..." -ForegroundColor Yellow
try {
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query "SELECT 1" -ErrorAction Stop | Out-Null
    Write-Host "Connected successfully" -ForegroundColor Green
}
catch {
    Write-Host "Failed to connect: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "Running reset script..." -ForegroundColor Yellow
try {
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $SqlScriptPath -ErrorAction Stop
    Write-Host "Database reset completed" -ForegroundColor Green
}
catch {
    Write-Host "Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "Verifying..." -ForegroundColor Yellow
$userCount = (Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query "SELECT COUNT(*) as Count FROM Users").Count
$roleCount = (Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query "SELECT COUNT(*) as Count FROM Roles").Count
Write-Host "Users: $userCount, Roles: $roleCount" -ForegroundColor Green

Write-Host ""
Write-Host "Now restart the API to trigger seeders" -ForegroundColor Cyan
Write-Host "Login credentials:" -ForegroundColor Yellow
Write-Host "  owner@clinic.com / ClinicOwner123!" -ForegroundColor White
Write-Host "  superadmin@clinic.com / SuperAdmin123!" -ForegroundColor White
