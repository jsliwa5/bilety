using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PTickets.Modules.Inspections.Domain;
using PTickets.Modules.Inspections.Infrastructure.Persistence;
using PTickets.Shared;
using PTickets.Shared.ValueObjects;

var services = new ServiceCollection();

services.AddLogging(builder => {
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug); // To see SQL
});

services.AddDbContext<InspectionsDbContext>(options =>
    options.UseSqlite("Data Source=../src/PTickets.Api/ptickets_inspections.db")
           .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

services.AddScoped<IInspectionRepository, InspectionRepository>();

var provider = services.BuildServiceProvider();
var repo = provider.GetRequiredService<IInspectionRepository>();

var inspection = Inspection.Create(
    SessionId.New(),
    InspectorId.New(),
    new RegistrationNumber("WZW-FAIL"),
    ZoneId.New(),
    StreetId.New(),
    50.0,
    20.0,
    DateTime.UtcNow);

await repo.AddAsync(inspection, default);
await repo.SaveChangesAsync(default);

Console.WriteLine("Created: " + inspection.Id.Value);

var loaded = await repo.GetByIdAsync(inspection.Id, default);
Console.WriteLine("Loaded: " + loaded.Id.Value);

// This triggers the bug
loaded.RecordTicketCheck(TicketCheckResult.Invalid("Mock"), true);

try {
    await repo.SaveChangesAsync(default);
    Console.WriteLine("Saved successfully!");
} catch(Exception ex) {
    Console.WriteLine("ERROR: " + ex.ToString());
}
