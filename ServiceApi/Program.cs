using ServiceApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ServiceApi.Repositorys;
using ServiceApi.Data;
using ServiceApi.Contacts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Access the configuration
var configuration = builder.Configuration;
// Read the connection string from appsettings.json
var connString = configuration.GetConnectionString("DefaultConnection");
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
     .AddSqlServer(
        connectionString: connString,
        name: "sql_server_health_check",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql", "critical" }
    )
    .AddUrlGroup(
        new Uri("https://another-service.com/status"),
        name: "Another Service Health Check",
        failureStatus: HealthStatus.Degraded,
        timeout: TimeSpan.FromSeconds(10)
    );



// Register the custom publisher for health checks
builder.Services.AddSingleton<IHealthCheckPublisher, DatabaseHealthCheckPublisher>();

// Configure the application to run health checks periodically
builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(5);  // Delay between health check runs
    options.Predicate = (check) => true;      // Run all health checks
    options.Period = TimeSpan.FromMinutes(1); // Period for running checks
});



// JWT Authentication setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        // Specify the issuer, audience, and secret key
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Configure Dependency Injection for services and repositories
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Configure Dapper with SQL Server
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add JWT Authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your JWT token in the format: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
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

builder.Services.AddAuthorization();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Use Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();
//app.MapHealthChecks("/health");

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var response = new
        {
            status = report.Status.ToString(),
            //totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description ?? "No description",
                duration = e.Value.Duration.TotalMilliseconds,  // Duration of individual health check
                tags = e.Value.Tags,                             // Tags associated with the health check
                exception = e.Value.Exception?.Message,          // Exception message if any occurred
                data = e.Value.Data                             // Additional data from the health check (if any)
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds // Include total duration for the entire health check
        };

        // Set the response content type to JSON
        context.Response.ContentType = "application/json";

        // Customize the JSON serialization options
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Serialize the response object to JSON and write it to the HTTP response
        var result = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(result);
    }
});

app.MapControllers();

app.Run();
