using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ApiClientFactory : IApiClientFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClientFactory(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public HttpClient CreateClient(string clientId)
        {
            var baseUrl = clientId switch
            {
                "api-stag" => "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion/",
                "api-prod" => "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion/",
                _ => throw new ArgumentException($"Unsupported client ID: {clientId}")
            };

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}
