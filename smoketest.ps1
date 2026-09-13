$ErrorActionPreference = 'Stop'

Write-Host "Starting Smoke Test..."

$zones = Invoke-RestMethod -Uri http://localhost:5000/api/zones
$zoneId = $zones[0].id
$streetId = $zones[0].streets[0].id

$violations = Invoke-RestMethod -Uri http://localhost:5000/api/violation-types
$violationId = $violations[0].id.value

$body = @{ firstName = "Jan"; lastName = "Kowalski" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri http://localhost:5000/api/inspectors -Method Post -Body $body -ContentType "application/json"
$inspectorId = $response.id

# 1. Start Session
$body = @{ inspectorId = $inspectorId } | ConvertTo-Json
$sessionId = Invoke-RestMethod -Uri http://localhost:5000/api/sessions -Method Post -Body $body -ContentType "application/json"
Write-Host "1. Session started: $sessionId"

# 2. Select street and zone
$body = @{ zoneId = $zoneId; streetId = $streetId } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5000/api/sessions/$sessionId/street -Method Put -Body $body -ContentType "application/json"
Write-Host "2. Street and Zone selected"

$invalidTicketFound = $false
$inspectionId = ""

while (-not $invalidTicketFound) {
    # Generate random registration
    $regNum = "WZW" + (Get-Random -Minimum 1000 -Maximum 9999)

    # 3. Start Inspection
    $body = @{ sessionId = $sessionId; registrationNumber = $regNum; latitude = 51.0; longitude = 17.0 } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri http://localhost:5000/api/inspections -Method Post -Body $body -ContentType "application/json"
    $inspectionId = $response
    Write-Host "3. Inspection started: $inspectionId (Reg: $regNum)"

    # 4. Check Ticket
    $checkResult = Invoke-RestMethod -Uri http://localhost:5000/api/inspections/$inspectionId/check-ticket -Method Post -ContentType "application/json"
    Write-Host "4. Ticket checked: $($checkResult.isValid)"

    if (-not $checkResult.isValid) {
        $invalidTicketFound = $true
    } else {
        Write-Host "Ticket was valid, restarting inspection..."
    }
}

# 5. Second Check
Write-Host "Waiting 1 second before second check..."
Start-Sleep -Seconds 1
$secondCheckResult = Invoke-RestMethod -Uri http://localhost:5000/api/inspections/$inspectionId/second-check -Method Post -ContentType "application/json"
Write-Host "5. Second check conducted: $($secondCheckResult.isValid)"

if ($secondCheckResult.isValid) {
    Write-Host "Second check found valid ticket. Cannot proceed with violation and notice."
    exit 0
}

# 6. Add Violation
$body = @{ violationTypeId = $violationId } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5000/api/inspections/$inspectionId/violations -Method Post -Body $body -ContentType "application/json"
Write-Host "6. Violation added"

# 7. Attach Photos
$photoId = [guid]::NewGuid().ToString()
$body = @{ fileIds = @($photoId) } | ConvertTo-Json
Invoke-RestMethod -Uri http://localhost:5000/api/inspections/$inspectionId/photos -Method Post -Body $body -ContentType "application/json"
Write-Host "7. Photos attached"

# 8. Issue Notice
$noticeId = Invoke-RestMethod -Uri http://localhost:5000/api/inspections/$inspectionId/notice -Method Post -ContentType "application/json"
Write-Host "8. Notice issued: $noticeId"

# 9. Verify Notice
$notice = Invoke-RestMethod -Uri http://localhost:5000/api/notices/$noticeId
Write-Host "9. Notice verification:"
$notice | ConvertTo-Json

Write-Host "Smoke Test Completed Successfully!"
