using System.Net.Http.Metrics;
using System.Text;
using System.Text.Json.Serialization;
using Andromeda.Features.Auth.IssueToken;
using Andromeda.Features.CreateSatellite;
using Andromeda.Features.DeleteSatellite;
using Andromeda.Features.GetAllSatellites;
using Andromeda.Features.GetSatellitesById;
using Andromeda.Features.UpdateSatellite;
using Andromeda.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("SatelliteDb")));

builder.Services.AddScoped<ISatelliteRepository, SatelliteRepository>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14);
});

// Configure JWT token and authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});
builder.Services.AddAuthorization();

builder.Services.AddHostedService<CacheWarmingService>();

builder.Host.ConfigureHostOptions(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Gracefully shutdown server
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Application is shutting down. Waiting for in-flight requests to complete.");
});

lifetime.ApplicationStopped.Register(() =>
{
    Log.Information("Application has stopped.");
});

// Logging and exception middleware
app.UseExceptionHandler();
app.UseMiddleware<RequestIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.MapCreateSatellite();
app.MapGetAllSatellites();
app.MapGetSatelliteById();
app.MapUpdateSatellite();
app.MapDeleteSatellite();
app.MapIssueToken();

app.Run();

