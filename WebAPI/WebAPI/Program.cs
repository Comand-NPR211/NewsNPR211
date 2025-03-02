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

// **��������� �������� ������� �����**
AppContext.SetSwitch("Switch.Microsoft.IdentityModel.UnsafeRelaxHmacKeySizeValidation", true);

// **�������� ����� ���������� �� ��**
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// **������������ ���� ����� (PostgreSQL)**
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// **��������� ������ ��� ������ �� ������������**
builder.Services.AddScoped<IImageHulk, ImageHulk>();

// **������ AutoMapper**
builder.Services.AddAutoMapper(typeof(MapProfile));

// **����������� Identity**
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// **JWT ������������**
var jwtKey = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Secret is missing in appsettings.json");
}

// ��� ��� �������� ������� �����
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
Console.WriteLine($"JWT Secret Length: {keyBytes.Length * 8} bits");

// **������������� ��������� ���� ��� ���������� ���**
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

// **������ ������� ����������� ��� �����**
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// **������������ CORS (���� �������� ���� ������)**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// **�������� ��������� ����� ��� ���������� �����**
string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploading");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

// **������������ JSON-����������**
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

    // ������ ��������� ����������� ����� JWT
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

// **����������� ����� ��� ����� ��������**
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

// **������������ Middleware**
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // ������ CORS
app.UseAuthentication(); // ������ ��������������
app.UseAuthorization();

app.MapControllers();
app.Run();
