using Application.Behaviors;
using Application.Interfaces;
using Application.Models;
using Application.Services;
using Application.Validators;
using Infrastructure.Database;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using Application.Features.GenerateXml;
using System.Net.Http.Headers;

namespace Infrastructure.DI
{
    public static class DependencyInjection
    {
        // Método para registrar servicios de la capa Application
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IFileStorage, FileStorage>();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddSingleton<IJWTService, JwtService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPasswordService, BCryptPasswordService>();

            // Configuración de FluentValidation (requiere el paquete)
            services.AddValidatorsFromAssemblyContaining<GetClaveQueryValidator>(); // <-- Ahora funcionará

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddValidatorsFromAssemblyContaining<GenerateXmlFeValidator>();
            services.AddScoped<GenerateXmlFeUseCase>();

            services.AddHttpClient<IXmlSignerService, XmlSignerService>(client => {
                client.BaseAddress = new Uri("http://php-apache/");
                client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.43.0"); 
            });

            services.AddHttpClient();
            services.AddSingleton<IApiClientFactory, ApiClientFactory>();
            services.AddScoped<IReceiptSender, ReceiptSenderService>();
            services.AddScoped<IReceiptRepository, ReceiptRepository>();

            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IInvoiceSearchService, InvoiceSearchService>();
        }

        // Método para registrar servicios de la capa Domain
        public static void AddDomainServices(this IServiceCollection services)
        {

        }

        // Método para registrar servicios de la capa Infrastructure (repositorios, contexto de base de datos, etc.)
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<DbConnectionFactory>();
            services.AddHttpClient<ITokenRepository, TokenRepository>();
        }
    }
}
