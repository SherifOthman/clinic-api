$base = "http://localhost:5000/api"
$pass = 0; $fail = 0

function Req($method, $url, $tok, $body) {
    try {
        $h = if ($tok) { @{"Authorization"="Bearer $tok"} } else { @{} }
        $p = @{ Uri=$url; Method=$method; Headers=$h; UseBasicParsing=$true }
        if ($body) { $p.Body = $body; $p.ContentType = "application/json" }
        [int](Invoke-WebRequest @p).StatusCode
    } catch { [int]$_.Exception.Response.StatusCode }
}

function Check($label, $status, $expected) {
    $ok = if ($expected -is [array]) { $expected -contains $status } else { $status -eq $expected }
    if ($ok) { $script:pass++ } else { $script:fail++ }
    $icon = if ($ok) { "PASS" } else { "FAIL" }
    "[$icon] [$status] $label"
}

function Perms($tok) {
    $p = $tok.Split('.')[1]; $p = $p + ('=' * ((4 - $p.Length % 4) % 4))
    (([System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($p)) | ConvertFrom-Json).permissions) -join ", "
}

# Login
$ot = (Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"emailOrUsername":"owner@clinic.com","password":"ClinicOwner123!"}' -Headers @{"X-Client-Type"="mobile"}).accessToken
$dt = (Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"emailOrUsername":"doctor@clinic.com","password":"Doctor123!"}' -Headers @{"X-Client-Type"="mobile"}).accessToken
$rt = (Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"emailOrUsername":"receptionist@clinic.com","password":"Receptionist123!"}' -Headers @{"X-Client-Type"="mobile"}).accessToken

# Get staff IDs and set permissions
$staff = Invoke-RestMethod -Uri "$base/staff" -Headers @{"Authorization"="Bearer $ot"}
$docId = ($staff.items | Where-Object { $_.roles -match "Doctor" -and $_.roles -notmatch "Owner" } | Select-Object -First 1).id
$recId = ($staff.items | Where-Object { $_.roles -match "Receptionist" } | Select-Object -First 1).id

Invoke-RestMethod -Uri "$base/staff/$docId/permissions" -Method PUT -ContentType "application/json" -Body '["ViewPatients","EditPatient","ManageSchedule","ManageVisitTypes","ViewAppointments","ManageAppointments"]' -Headers @{"Authorization"="Bearer $ot"} | Out-Null
Invoke-RestMethod -Uri "$base/staff/$recId/permissions" -Method PUT -ContentType "application/json" -Body '["ViewPatients","ViewInvoices"]' -Headers @{"Authorization"="Bearer $ot"} | Out-Null

# Re-login for fresh tokens with updated permissions
$dt = (Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"emailOrUsername":"doctor@clinic.com","password":"Doctor123!"}' -Headers @{"X-Client-Type"="mobile"}).accessToken
$rt = (Invoke-RestMethod -Uri "$base/auth/login" -Method POST -ContentType "application/json" -Body '{"emailOrUsername":"receptionist@clinic.com","password":"Receptionist123!"}' -Headers @{"X-Client-Type"="mobile"}).accessToken

$patientData = Invoke-RestMethod -Uri "$base/patients/$patientId" -Headers @{"Authorization"="Bearer $ot"}
$putBody = "{`"firstName`":`"$($patientData.firstName)`",`"lastName`":`"$($patientData.lastName)`",`"dateOfBirth`":`"$($patientData.dateOfBirth)`",`"gender`":`"$($patientData.gender)`",`"phoneNumbers`":[],`"chronicDiseaseIds`":[]}"

"Doctor JWT permissions:       $(Perms $dt)"
"Receptionist JWT permissions: $(Perms $rt)"
""

# OWNER
"=== OWNER (all permissions) ==="
Check "GET  /patients"           (Req GET    "$base/patients?pageNumber=1&pageSize=1" $ot $null) 200
Check "GET  /branches"           (Req GET    "$base/branches" $ot $null) 200
Check "GET  /staff"              (Req GET    "$base/staff" $ot $null) 200
Check "POST /staff/invite"       (Req POST   "$base/staff/invite" $ot '{"email":"newtest@test.com","role":"Doctor"}') @(200,400)

""
"=== DOCTOR (ViewPatients+EditPatient+ManageSchedule - no Create/Delete/Staff/Branches) ==="
Check "GET  /patients       (allow)" (Req GET    "$base/patients?pageNumber=1&pageSize=1" $dt $null) 200
Check "POST /patients       (deny)"  (Req POST   "$base/patients" $dt '{"firstName":"T","lastName":"P","gender":"Male","phoneNumbers":[]}') 403
Check "PUT  /patients/:id   (allow)" (Req PUT    "$base/patients/$patientId" $dt $putBody) 204
Check "DELETE /patients/:id (deny)"  (Req DELETE "$base/patients/$patientId" $dt $null) 403
Check "GET  /branches       (deny)"  (Req GET    "$base/branches" $dt $null) 403
Check "POST /branches       (deny)"  (Req POST   "$base/branches" $dt '{"name":"T"}') 403
Check "GET  /staff          (deny)"  (Req GET    "$base/staff" $dt $null) @(401,403)
Check "POST /staff/invite   (deny)"  (Req POST   "$base/staff/invite" $dt '{"email":"x@x.com","role":"Doctor"}') @(401,403)

""
"=== RECEPTIONIST (ViewPatients+ViewInvoices only) ==="
Check "GET  /patients       (allow)" (Req GET    "$base/patients?pageNumber=1&pageSize=1" $rt $null) 200
Check "POST /patients       (deny)"  (Req POST   "$base/patients" $rt '{"firstName":"T","lastName":"P","gender":"Male","phoneNumbers":[]}') 403
Check "PUT  /patients/:id   (deny)"  (Req PUT    "$base/patients/$patientId" $rt $putBody) 403
Check "DELETE /patients/:id (deny)"  (Req DELETE "$base/patients/$patientId" $rt $null) 403
Check "GET  /branches       (deny)"  (Req GET    "$base/branches" $rt $null) 403
Check "GET  /staff          (deny)"  (Req GET    "$base/staff" $rt $null) @(401,403)
Check "POST /staff/invite   (deny)"  (Req POST   "$base/staff/invite" $rt '{"email":"x@x.com","role":"Doctor"}') @(401,403)

""
"=== UNAUTHENTICATED ==="
Check "GET  /patients            (deny)"  (Req GET "$base/patients?pageNumber=1&pageSize=1" $null $null) 401
Check "GET  /locations/countries (allow)" (Req GET "$base/locations/countries" $null $null) 200
Check "GET  /specializations     (allow)" (Req GET "$base/specializations" $null $null) 200
Check "GET  /subscription-plans  (allow)" (Req GET "$base/subscription-plans" $null $null) 200

""
"======================================"
"RESULTS: $pass passed, $fail failed out of $($pass + $fail) tests"
"======================================"

