using Infrastructure.Database;
using Infrastructure.DI;
using Scalar.AspNetCore;
using MediatR;
using Application.Features.Files;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Application.Features.GenerateClave;
using Application.Features.SignDocument;

var builder = WebApplication.CreateBuilder(args);

// Register your services
builder.Services.AddApplicationServices(builder.Configuration); // Application layer services
builder.Services.AddDomainServices(); // Domain layer services
builder.Services.AddInfrastructureServices(); // Infrastructure layer services (including Dapper)


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Swagger services
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(); // Add this line

// Registrar DatabaseInitializer
builder.Services.AddHostedService<DatabaseInitializer>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UploadCertCommand).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClaveQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SignElectronicDocumentCommand).Assembly));


// Autenticaci�n
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //app.UseSwagger();  // Enable Swagger middleware
    //app.UseSwaggerUI();  // Enable Swagger UI
    app.MapScalarApiReference(options =>
    {
        options.
                WithTitle("Ledger Sync API")
                .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.HttpClient);
    }); //using scalar
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
