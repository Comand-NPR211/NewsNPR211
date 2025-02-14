using Microsoft.EntityFrameworkCore;
using WebAlina.Services;
using WebAPI.Data;
using WebAPI.Interfaces;
using WebAPI.Mapper;

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

builder = WebApplication.CreateBuilder(args);

// Реєстрація вашого сервісу IImageHulk
builder.Services.AddScoped<IImageHulk, ImageHulk>();

// Додавання інших сервісів
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IImageHulk, ImageHulk>();

builder.Services.AddAutoMapper(typeof(MapProfile));

builder.Services.AddControllersWithViews();

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
