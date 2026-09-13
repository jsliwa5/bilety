using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Inspections.Infrastructure.Persistence;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

var services = new ServiceCollection();
services.AddDbContext<InspectionsDbContext>(options =>
    options.UseSqlite("Data Source=ptickets_inspections.db"));
services.AddScoped<IInspectionRepository, InspectionRepository>();

var provider = services.BuildServiceProvider();
var repo = provider.GetRequiredService<IInspectionRepository>();

var inspection = Inspection.Create(
    SessionId.New(),
    InspectorId.New(),
    new RegistrationNumber("WZW-TEST"),
    ZoneId.New(),
    StreetId.New(),
    50.0,
    20.0,
    DateTime.UtcNow);

await repo.AddAsync(inspection, default);
await repo.SaveChangesAsync(default);

Console.WriteLine("Created: " + inspection.Id.Value);

var loaded = await repo.GetByIdAsync(inspection.Id, default);
loaded.Approve(); // Changes Status to Approved
await repo.SaveChangesAsync(default);
Console.WriteLine("Status updated successfully!");

loaded.RecordTicketCheck(TicketCheckResult.Valid(DateTime.UtcNow, DateTime.UtcNow.AddHours(1), "Test"), true);
await repo.SaveChangesAsync(default);
Console.WriteLine("TicketResult updated successfully!");
