using System.Text.Json.Serialization;
using JobTracker.Api.Features.Applications;
using JobTracker.Api.Features.Companies;
using JobTracker.Api.Infrastructure;
using JobTracker.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Local development reads secrets from the same .env file docker compose uses.
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddDotEnvFile(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(ConnectionStringResolver.Resolve(builder.Configuration)));

builder.Services.AddValidation();

// The Blazor WebAssembly client is served from its own origin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapCompanyEndpoints();
app.MapJobApplicationEndpoints();

app.Run();
