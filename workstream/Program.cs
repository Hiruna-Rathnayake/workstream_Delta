using Microsoft.EntityFrameworkCore;
using workstream.Data;
using workstream.Profiles;
using AutoMapper;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using workstream.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 10 * 1024 * 1024, retainedFileCountLimit: 5)
    .CreateLogger();

builder.Services.AddLogging(logging =>
{
    logging.AddSerilog(); // Add Serilog as the logging provider
    logging.SetMinimumLevel(LogLevel.Information);
});

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger for API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper for DTO to Model mappings.
builder.Services.AddAutoMapper(typeof(MappingProfile)); // Ensure MappingProfile is properly set

// Register JwtService with configuration from appsettings.json
builder.Services.AddScoped<JwtService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var secretKey = configuration["Jwt:SecretKey"];
    var issuer = configuration["Jwt:Issuer"];
    var audience = configuration["Jwt:Audience"];

    // Get PermissionRepo from DI container
    var permissionRepo = provider.GetRequiredService<PermissionRepo>();

    return new JwtService(secretKey, issuer, audience, permissionRepo);
});

// Define your database connection string 
var connection = builder.Configuration.GetConnectionString("WorkstreamDbConnection")
                 ?? "Data Source=DESKTOP-CLS1LPH\\SQLEXPRESS;Initial Catalog=WorkstreamDb;Integrated Security=True;TrustServerCertificate=True;"; // Default fallback

// Add DbContext to the service container, using SQL Server.
builder.Services.AddDbContext<WorkstreamDbContext>(options =>
    options.UseSqlServer(connection));

// Add repositories to the container for dependency injection.
builder.Services.AddScoped<TenantRepo>();  // Add TenantRepo service
builder.Services.AddScoped<UserRepo>();    // Add UserRepo service
builder.Services.AddScoped<InventoryRepo>();
builder.Services.AddScoped<StockRepo>();
builder.Services.AddScoped<OrderRepo>();
builder.Services.AddScoped<CustomerRepo>();
builder.Services.AddScoped<RoleRepo>();
builder.Services.AddScoped<PermissionRepo>();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS policy
app.UseCors("AllowAll");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.Run();
