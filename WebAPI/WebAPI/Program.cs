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
using WebAPI.Middleware;
using WebAPI.Services;
using WebAPI.Middleware;
//using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Налаштування логування
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>() // поки скасовуємо більш жорсткі налаштування вимоги до пароля

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => //ставим до пароля менш жорсткі вимоги+унікальність мейла новгого юзера
{
    options.User.RequireUniqueEmail = true; 
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})

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

        // - обробка помилки для Swagger (не редірект, а 401)
        options.Events = new JwtBearerEvents
        {
            //  ЛОГУЄМО ПОМИЛКИ ПАРСИНГУ / ВАЛІДАЦІЇ ТОКЕНА
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Unauthorized access attempt. Token was missing or invalid.");

                context.HandleResponse();// запобігає редіректу
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new { error = "Unauthorized access" });
                return context.Response.WriteAsync(result);
            }
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

app.UseGlobalErrorHandler(); // глобальна обробка винятків


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
        //Console.WriteLine($"Error initializing roles: {ex.Message}");
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error initializing roles");
    }
}

// **Налаштування Middleware**  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAll"); // Додаємо CORS
app.UseAuthentication(); // Додаємо автентифікацію
app.UseAuthorization(); 

app.MapControllers();
Console.WriteLine(" MapControllers() applied");

var endpoints = app as IEndpointRouteBuilder;
foreach (var endpoint in endpoints.DataSources.SelectMany(source => source.Endpoints))
{
    Console.WriteLine(endpoint.DisplayName);
}

app.Run();
