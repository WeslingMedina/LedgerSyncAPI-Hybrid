using Application.Interfaces;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly HttpClient _httpClient;

        public InvoiceRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<InvoiceSearchResponse> GetInvoiceByKeyAsync(InvoiceSearchRequest request)
        {
            string? url = GetApiUrl(request.ClientId);
            if (url == null)
            {
                return new InvoiceSearchResponse
                {
                    Status = 400,
                    Data = "Ha ocurrido un error en el client_id."
                };
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.Token);
                _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

                var response = await _httpClient.GetAsync($"{url}{request.Clave}");
                var content = await response.Content.ReadAsStringAsync();

                return new InvoiceSearchResponse
                {
                    Status = (int)response.StatusCode,
                    Data = JsonSerializer.Deserialize<object>(content)
                };
            }
            catch (Exception ex)
            {
                return new InvoiceSearchResponse
                {
                    Status = 500,
                    Data = ex.Message
                };
            }
        }

        private string? GetApiUrl(string clientId)
        {
            return clientId switch
            {
                "api-stag" => "https://api-sandbox.comprobanteselectronicos.go.cr/recepcion/v1/recepcion/",
                "api-prod" => "https://api.comprobanteselectronicos.go.cr/recepcion/v1/recepcion/",
                _ => null
            };
        }
    }
}
