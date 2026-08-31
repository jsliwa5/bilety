using PTickets.Modules.FileStorage;
using PTickets.Modules.Inspections;
using PTickets.Modules.InspectorTracking;
using PTickets.Modules.Notices;
using PTickets.Modules.Notifications;
using PTickets.Modules.Tickets;
using PTickets.Modules.Violations;
using PTickets.Modules.Zones;
using PTickets.Shared.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

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

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapZonesEndpoints();
app.MapViolationsEndpoints();
app.MapInspectorTrackingEndpoints();
app.MapTicketsEndpoints();
app.MapInspectionsEndpoints();
app.MapNoticesEndpoints();
app.MapNotificationsEndpoints();

app.Run();

