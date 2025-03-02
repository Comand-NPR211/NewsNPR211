using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebAlina.Services;
using WebAPI.Data;
using WebAPI.Data.Entities;
using WebAPI.Interfaces;
using WebAPI.Mapper;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// **Вимкнення перевірки довжини ключа**
AppContext.SetSwitch("Switch.Microsoft.IdentityModel.UnsafeRelaxHmacKeySizeValidation", true);

// **Отримуємо рядок підключення до БД**
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// **Налаштування бази даних (PostgreSQL)**
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// **Реєстрація сервісу для роботи із зображеннями**
builder.Services.AddScoped<IImageHulk, ImageHulk>();

// **Додаємо AutoMapper**
builder.Services.AddAutoMapper(typeof(MapProfile));

// **Налаштовуємо Identity**
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// **JWT Налаштування**
var jwtKey = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Secret is missing in appsettings.json");
}

// Лог для перевірки довжини ключа
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
Console.WriteLine($"JWT Secret Length: {keyBytes.Length * 8} bits");

// **Використовуємо звичайний ключ без додаткових змін**
var signingKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// **Додаємо політику авторизації для ролей**
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// **Налаштування CORS (якщо фронтенд буде окремо)**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// **Перевірка існування папки для збереження файлів**
string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploading");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

// **Налаштування JSON-серіалізації**
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

// **Swagger**
builder.Services.AddEndpointsApiExplorer(); 

//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" });

    // Додаємо можливість авторизації через JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer {your token here}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// **Ініціалізація ролей при старті програми**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RolesInitializer.InitializeRoles(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing roles: {ex.Message}");
    }
}

// **Налаштування Middleware**
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // Додаємо CORS
app.UseAuthentication(); // Додаємо автентифікацію
app.UseAuthorization();

app.MapControllers();
app.Run();
