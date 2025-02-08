using Microsoft.EntityFrameworkCore;
using System;
using WebAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Email dajoh52087@nike4s.com
//Password dajoh52087@nike4s.comQWE
//postgresql://neondb_owner:@/?sslmode=require

// Get the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add the DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
