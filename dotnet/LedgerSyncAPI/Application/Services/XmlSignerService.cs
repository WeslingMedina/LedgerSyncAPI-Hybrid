using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Numerics;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

public class XmlSignerService : IXmlSignerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<XmlSignerService> _logger;

    public XmlSignerService(HttpClient httpClient, ILogger<XmlSignerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SigningResponse> SignDocument(SigningRequest request)
    {
        try
        {
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(request.Class), "w"); // Valor fijo para "w"
            formData.Add(new StringContent(request.Method), "r");   // Valor fijo para "r"
            formData.Add(new StringContent(request.P12Url), "p12Url");
            formData.Add(new StringContent(request.PinP12), "pinP12");
            formData.Add(new StringContent(request.InXml), "inXml");
            formData.Add(new StringContent(request.DocType), "tipodoc"); // "FE", "ND", etc.

            var response = await _httpClient.PostAsync("api.php", formData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SigningResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling PHP API");
            throw new ApplicationException("Error en el servicio de firma", ex);
        }
    }
}