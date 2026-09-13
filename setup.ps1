$ErrorActionPreference = 'Stop'
Write-Host "Setting up DB..."

$zones = Invoke-RestMethod -Uri http://localhost:5000/api/zones
$zoneId = $zones[0].id

if ($zoneId) {
    # Create Street
    $body = @{ name = "Marszalkowska" } | ConvertTo-Json
    $streetId = Invoke-RestMethod -Uri http://localhost:5000/api/zones/$zoneId/streets -Method Post -Body $body -ContentType "application/json"
    Write-Host "Street created: $streetId"
}

# Create Violation Type
$body = @{ name = "Brak biletu"; description = "Postoj bez waznego biletu" } | ConvertTo-Json
$violationId = Invoke-RestMethod -Uri http://localhost:5000/api/violation-types -Method Post -Body $body -ContentType "application/json"
Write-Host "Violation created: $violationId"

Write-Host "Setup done."
