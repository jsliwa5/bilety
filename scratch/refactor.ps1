$moduleDir = "src\Modules\Violations"

New-Item -ItemType Directory -Force -Path "$moduleDir\Common\Data"
New-Item -ItemType Directory -Force -Path "$moduleDir\Common\Exceptions"
New-Item -ItemType Directory -Force -Path "$moduleDir\CreateViolationType"
New-Item -ItemType Directory -Force -Path "$moduleDir\GetAllViolationTypes"
New-Item -ItemType Directory -Force -Path "$moduleDir\SetPenaltyAmount"
New-Item -ItemType Directory -Force -Path "$moduleDir\CreateSurchargeTier"
New-Item -ItemType Directory -Force -Path "$moduleDir\GetAllSurchargeTiers"

Move-Item -Path "$moduleDir\Domain\*.cs" -Destination "$moduleDir\Common\Data\"
Move-Item -Path "$moduleDir\Infrastructure\Persistence\ViolationsDbContext.cs" -Destination "$moduleDir\ViolationsDbContext.cs"

Move-Item -Path "$moduleDir\Application\Dtos\CreateViolationTypeRequest.cs" -Destination "$moduleDir\CreateViolationType\"
Move-Item -Path "$moduleDir\Application\Dtos\SetPenaltyAmountRequest.cs" -Destination "$moduleDir\SetPenaltyAmount\"
Move-Item -Path "$moduleDir\Application\Dtos\CreateSurchargeTierRequest.cs" -Destination "$moduleDir\CreateSurchargeTier\"

Move-Item -Path "$moduleDir\Application\ViolationsModuleFacade.cs" -Destination "$moduleDir\"
Move-Item -Path "$moduleDir\Application\Services\PenaltyCalculationService.cs" -Destination "$moduleDir\"

$createViolationType = @"
namespace PTickets.Modules.Violations.CreateViolationType;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Violations.Common.Data;

public static class CreateViolationTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost(""/violation-types"", async (CreateViolationTypeRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var violationType = ViolationType.Create(request.Name, request.Description);
            dbContext.ViolationTypes.Add(violationType);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($`"/api/violation-types/{violationType.Id.Value}`", new { id = violationType.Id.Value });
        });
    }
}
"@
Set-Content -Path "$moduleDir\CreateViolationType\CreateViolationTypeEndpoint.cs" -Value $createViolationType

$getAllViolationTypes = @"
namespace PTickets.Modules.Violations.GetAllViolationTypes;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Violations.Contracts;

public static class GetAllViolationTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet(""/violation-types"", async (IViolationsModule module, CancellationToken ct) =>
        {
            var violationTypes = await module.GetAllViolationTypesAsync(ct);
            return Results.Ok(violationTypes);
        });
    }
}
"@
Set-Content -Path "$moduleDir\GetAllViolationTypes\GetAllViolationTypesEndpoint.cs" -Value $getAllViolationTypes

$setPenaltyAmount = @"
namespace PTickets.Modules.Violations.SetPenaltyAmount;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations.Common.Data;
using PTickets.Shared;

public static class SetPenaltyAmountEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost(""/violation-types/{id:guid}/penalty-amount"", async (Guid id, SetPenaltyAmountRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var violationTypeId = new ViolationTypeId(id);
            var exists = await dbContext.ViolationTypes.AnyAsync(v => v.Id == violationTypeId, ct);
            if (!exists)
            {
                return Results.NotFound(new { message = $`"Violation type with ID {id} was not found.`" });
            }

            var penaltyAmount = PenaltyAmount.Create(violationTypeId, request.Amount, request.EffectiveFrom);
            dbContext.PenaltyAmounts.Add(penaltyAmount);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($`"/api/violation-types/{id}/penalty-amount/{penaltyAmount.Id}`", new { id = penaltyAmount.Id });
        });
    }
}
"@
Set-Content -Path "$moduleDir\SetPenaltyAmount\SetPenaltyAmountEndpoint.cs" -Value $setPenaltyAmount

$createSurchargeTier = @"
namespace PTickets.Modules.Violations.CreateSurchargeTier;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PTickets.Modules.Violations.Common.Data;

public static class CreateSurchargeTierEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost(""/surcharge-tiers"", async (CreateSurchargeTierRequest request, ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var tier = SurchargeTier.Create(request.MinMinutes, request.MaxMinutes, request.Amount);
            dbContext.SurchargeTiers.Add(tier);
            await dbContext.SaveChangesAsync(ct);

            return Results.Created($`"/api/surcharge-tiers/{tier.Id.Value}`", new { id = tier.Id.Value });
        });
    }
}
"@
Set-Content -Path "$moduleDir\CreateSurchargeTier\CreateSurchargeTierEndpoint.cs" -Value $createSurchargeTier

$getAllSurchargeTiers = @"
namespace PTickets.Modules.Violations.GetAllSurchargeTiers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PTickets.Modules.Violations;

public static class GetAllSurchargeTiersEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet(""/surcharge-tiers"", async (ViolationsDbContext dbContext, CancellationToken ct) =>
        {
            var tiers = await dbContext.SurchargeTiers
                .AsNoTracking()
                .OrderBy(t => t.MinMinutes)
                .Select(t => new
                {
                    id = t.Id.Value,
                    minMinutes = t.MinMinutes,
                    maxMinutes = t.MaxMinutes,
                    amount = t.Amount
                })
                .ToListAsync(ct);

            return Results.Ok(tiers);
        });
    }
}
"@
Set-Content -Path "$moduleDir\GetAllSurchargeTiers\GetAllSurchargeTiersEndpoint.cs" -Value $getAllSurchargeTiers

$moduleSetup = Get-Content "$moduleDir\ViolationsModule.cs" -Raw
$moduleSetup = $moduleSetup -replace 'using PTickets\.Modules\.Violations\.Endpoints;.*', ""
$moduleSetup = $moduleSetup -replace 'using PTickets\.Modules\.Violations\.Application\.Services;', ""
$moduleSetup = $moduleSetup -replace 'using PTickets\.Modules\.Violations\.Infrastructure\.Persistence;', "using Microsoft.EntityFrameworkCore;`r`nusing PTickets.Modules.Violations.CreateViolationType;`r`nusing PTickets.Modules.Violations.GetAllViolationTypes;`r`nusing PTickets.Modules.Violations.SetPenaltyAmount;`r`nusing PTickets.Modules.Violations.CreateSurchargeTier;`r`nusing PTickets.Modules.Violations.GetAllSurchargeTiers;"
$moduleSetup = $moduleSetup -replace 'ViolationsEndpoints\.MapViolationsEndpoints\(endpoints\);', "var group = endpoints.MapGroup(""/api"");`r`n        CreateViolationTypeEndpoint.Map(group);`r`n        GetAllViolationTypesEndpoint.Map(group);`r`n        SetPenaltyAmountEndpoint.Map(group);`r`n        CreateSurchargeTierEndpoint.Map(group);`r`n        GetAllSurchargeTiersEndpoint.Map(group);"
Set-Content -Path "$moduleDir\ViolationsModule.cs" -Value $moduleSetup

Remove-Item -Path "$moduleDir\Domain" -Recurse -Force
Remove-Item -Path "$moduleDir\Infrastructure" -Recurse -Force
Remove-Item -Path "$moduleDir\Application" -Recurse -Force
Remove-Item -Path "$moduleDir\Endpoints" -Recurse -Force

$files = Get-ChildItem -Path "$moduleDir" -Recurse -Filter "*.cs" -Exclude "*DbContextModelSnapshot.cs", "*.Designer.cs"
foreach ($file in $files) {
    if ($file.FullName -match "\\PTickets.Modules.Violations.Contracts\\") { continue }
    $content = Get-Content $file.FullName -Raw
    
    $content = $content -replace 'namespace PTickets\.Modules\.Violations\.Domain;', 'namespace PTickets.Modules.Violations.Common.Data;'
    $content = $content -replace 'namespace PTickets\.Modules\.Violations\.Application\.Dtos;', 'namespace PTickets.Modules.Violations.CreateViolationType;'
    
    $content = $content -replace 'using PTickets\.Modules\.Violations\.Domain;', 'using PTickets.Modules.Violations.Common.Data;'
    $content = $content -replace 'using PTickets\.Modules\.Violations\.Infrastructure\.Persistence;', 'using PTickets.Modules.Violations;'
    $content = $content -replace 'namespace PTickets\.Modules\.Violations\.Infrastructure\.Persistence;', 'namespace PTickets.Modules.Violations;'
    $content = $content -replace 'namespace PTickets\.Modules\.Violations\.Application\.Services;', 'namespace PTickets.Modules.Violations;'
    $content = $content -replace 'namespace PTickets\.Modules\.Violations\.Application;', 'namespace PTickets.Modules.Violations;'
    
    Set-Content -Path $file.FullName -Value $content -NoNewline
}

$content = Get-Content "$moduleDir\CreateViolationType\CreateViolationTypeRequest.cs" -Raw
$content = $content -replace 'namespace PTickets\.Modules\.Violations\.CreateViolationType;', 'namespace PTickets.Modules.Violations.CreateViolationType;'
Set-Content -Path "$moduleDir\CreateViolationType\CreateViolationTypeRequest.cs" -Value $content

$content = Get-Content "$moduleDir\SetPenaltyAmount\SetPenaltyAmountRequest.cs" -Raw
$content = $content -replace 'namespace PTickets\.Modules\.Violations\.CreateViolationType;', 'namespace PTickets.Modules.Violations.SetPenaltyAmount;'
Set-Content -Path "$moduleDir\SetPenaltyAmount\SetPenaltyAmountRequest.cs" -Value $content

$content = Get-Content "$moduleDir\CreateSurchargeTier\CreateSurchargeTierRequest.cs" -Raw
$content = $content -replace 'namespace PTickets\.Modules\.Violations\.CreateViolationType;', 'namespace PTickets.Modules.Violations.CreateSurchargeTier;'
Set-Content -Path "$moduleDir\CreateSurchargeTier\CreateSurchargeTierRequest.cs" -Value $content
