# Script to run all DbMigrators in sequence

Write-Host "Starting database migrations for all services..." -ForegroundColor Green

$services = @(
    "AuthServer",
    "AdministrationService",
    "IdentityService"
)

foreach ($service in $services) {
    Write-Host "`nMigrating database for $service..." -ForegroundColor Cyan
    
    $migrationPath = ".\services\$service\$service.DbMigrator"
    
    if (Test-Path $migrationPath) {
        Push-Location $migrationPath
        try {
            dotnet run
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Successfully migrated database for $service" -ForegroundColor Green
            } else {
                Write-Host "Failed to migrate database for $service" -ForegroundColor Red
                exit $LASTEXITCODE
            }
        }
        finally {
            Pop-Location
        }
    } else {
        Write-Host "Migration project not found for $service at path: $migrationPath" -ForegroundColor Yellow
    }
}

Write-Host "`nAll database migrations completed successfully!" -ForegroundColor Green
