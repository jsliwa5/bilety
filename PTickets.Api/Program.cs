using Microsoft.EntityFrameworkCore;
using PTickets.Api.Common;
using PTickets.Api.Database;
using PTickets.Api.Inspections;
using PTickets.Api.Inspections.ConductInspection;
using PTickets.Api.Inspectors;
using PTickets.Api.Inspectors.AddInspector;
using PTickets.Api.Zones;
using PTickets.Api.Zones.AddZone;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ParkingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddZonesServices();
builder.Services.AddInspectorsServices();
builder.Services.AddInspectionsServices();
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapConductInspection();
app.MapAddInspector();
app.MapAddZone();



app.Run();
