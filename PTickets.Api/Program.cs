using Microsoft.EntityFrameworkCore;
using PTickets.Api.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ParkingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Database")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.Run();
