using Microsoft.EntityFrameworkCore;
using PTickets.Modules.FileStorage;
using PTickets.Modules.Inspections;
using PTickets.Modules.InspectorTracking;
using PTickets.Modules.Notices;
using PTickets.Modules.Notifications;
using PTickets.Modules.Tickets;
using PTickets.Modules.Violations;
using PTickets.Modules.Zones;
using PTickets.Shared.Abstractions;
using PTickets.Api.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

// Swagger z grupowaniem per moduł
builder.Services.AddSwaggerGen(c =>
{
    c.TagActionsBy(api =>
    {
        var path = api.RelativePath ?? "";
        var segments = path.Split('/');
        if (segments.Length >= 2)
            return new[] { segments[1].Replace("-", " ").ToUpper() };
        return new[] { "OTHER" };
    });
});

builder.Services.AddControllers();
builder.Services.AddCors();

// MediatR – skanuje wszystkie moduły
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(ZonesModule).Assembly,
    typeof(ViolationsModule).Assembly,
    typeof(InspectorTrackingModule).Assembly,
    typeof(TicketsModule).Assembly,
    typeof(InspectionsModule).Assembly,
    typeof(NoticesModule).Assembly,
    typeof(NotificationsModule).Assembly,
    typeof(FileStorageModule).Assembly
));

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

// Rejestracja modułów
builder.Services.AddZonesModule(builder.Configuration);
builder.Services.AddViolationsModule(builder.Configuration);
builder.Services.AddInspectorTrackingModule(builder.Configuration);
builder.Services.AddTicketsModule(builder.Configuration);
builder.Services.AddInspectionsModule(builder.Configuration);
builder.Services.AddNoticesModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddFileStorageModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

// Auto-create databases (dev only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;

    // EnsureCreated for each module's DbContext
    sp.GetRequiredService<ZonesDbContext>().Database.EnsureCreated();
    sp.GetRequiredService<PTickets.Modules.Violations.ViolationsDbContext>().Database.EnsureCreated();
    sp.GetRequiredService<PTickets.Modules.Inspections.Infrastructure.Persistence.InspectionsDbContext>().Database.EnsureCreated();
    sp.GetRequiredService<NoticesDbContext>().Database.EnsureCreated();
    sp.GetRequiredService<PTickets.Modules.Notifications.Infrastructure.Persistence.NotificationsDbContext>().Database.EnsureCreated();
    sp.GetRequiredService<PTickets.Modules.Tickets.Infrastructure.Persistence.TicketsDbContext>().Database.EnsureCreated();
    sp.GetRequiredService<PTickets.Modules.FileStorage.Infrastructure.Persistence.FileStorageDbContext>().Database.EnsureCreated();
    // InspectorTrackingDbContext is internal – resolved via generic method
    var itDbType = typeof(PTickets.Modules.InspectorTracking.InspectorTrackingModule).Assembly
        .GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(DbContext)) && !t.IsAbstract);
    if (itDbType != null)
    {
        var itDb = (DbContext)sp.GetRequiredService(itDbType);
        itDb.Database.EnsureCreated();
    }

    var mediator = sp.GetRequiredService<MediatR.IMediator>();
    var zonesDb = sp.GetRequiredService<ZonesDbContext>();
    await ZonesSeeder.SeedAsync(zonesDb, mediator);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PTickets API v1");
    c.DocumentTitle = "PTickets – Swagger UI";
});

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();
app.MapZonesEndpoints();
app.MapViolationsEndpoints();
app.MapInspectorTrackingEndpoints();
app.MapTicketsEndpoints();
app.MapInspectionsEndpoints();
app.MapNoticesEndpoints();
app.MapNotificationsEndpoints();

app.Run();
