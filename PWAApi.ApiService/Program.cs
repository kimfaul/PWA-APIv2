using Microsoft.EntityFrameworkCore;
using EventApi.Data;
using API.Services.PlantID;
using Microsoft.AspNetCore.Http.Features;
using AutoMapper;
using PWAApi.ApiService.Services.AI;
using PWAApi.ApiService.Services.PlantInfo;
using PWAApi.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Configure the HTTP request pipeline.
builder.Services.AddHttpClient();

//Distributed Cache where we can add/remove items to/from the Redis cache
builder.AddRedisDistributedCache("cache");

// Add services to the container.
builder.Services.AddProblemDetails();

// Register your database context (change connection string as needed)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register AutoMapper (scanning all assemblies for profiles)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register repository and service for dependency injection
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAIService, OpenAIService>();
builder.Services.AddScoped<WateringScheduleService>();
builder.Services.AddScoped<WikimediaService>();

// Set API providers from configuration
var plantIDAPIProvider = builder.Configuration["PlantIDProvider"];
switch (plantIDAPIProvider)
{
    case "PlantNet":
    default:
        builder.Services.AddScoped<IPlantIDService, PlantNetService>();
        break;
}

var plantInfoAPIProvider = builder.Configuration["PlantInfoProvider"];
switch (plantInfoAPIProvider)
{
    case "AI":
    default:
        builder.Services.AddScoped<IPlantInfoService, AIPlantInfoService>();
        break;
}

// Load user secrets
builder.Configuration.AddUserSecrets<Program>();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger (if you're using it for API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50_000_000; // 50 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Enable Swagger (only for development or debugging)
    app.UseSwagger();
    app.UseSwaggerUI();

    // Check AutoMapper configuration
    try
    {
        var mapper = app.Services.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"AutoMapper configuration error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline for production
app.UseCors(MyAllowSpecificOrigins);

// Enable authorization (if applicable)
app.UseAuthorization();

// Map controllers (API endpoints)
// In previous version of ASP.NET Core (3-5), you would need to call UseRouting and UseEndpoints explicitly
// But in ASP.NET Core 6+, the MapControllers method is sufficient to set up routing for attribute-routed controllers
app.MapControllers();

app.Run();
