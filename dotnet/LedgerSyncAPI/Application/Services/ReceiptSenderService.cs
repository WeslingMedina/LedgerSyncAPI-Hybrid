using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ReceiptSenderService : IReceiptSender
    {
        private readonly IApiClientFactory _apiClientFactory;
        private readonly ILogger<ReceiptSenderService> _logger;

        public ReceiptSenderService(IApiClientFactory apiClientFactory, ILogger<ReceiptSenderService> logger)
        {
            _apiClientFactory = apiClientFactory;
            _logger = logger;
        }

        public async Task<ApiResponseDto> SendReceiptAsync(ReceiptSendDto dto)
        {
            try
            {
                var request = BuildReceiptRequest(dto);
                var client = _apiClientFactory.CreateClient(dto.ClientId);

                var requestJson = JsonSerializer.Serialize(request);
                _logger.LogDebug("JSON: {Json}", requestJson);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "")
                {
                    Content = content,
                    Headers =
                    {
                        { "Authorization", $"bearer {dto.Token}" }
                    }
                };

                using var response = await client.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                var responseLines = new List<string>();

                // Access the 'x-amzn-errortype' header
                if (statusCode != 202 && response.Headers.TryGetValues("x-amzn-errortype", out var errorTypeValues))
                {
                    responseLines.Add(errorTypeValues.FirstOrDefault());
                }else if (statusCode == 202)
                {
                    responseLines.Add("Accepted");
                }else if (statusCode != 202 && response.Headers.TryGetValues("x-error-cause", out var errorCauseValues))
                {
                    responseLines.Add(errorCauseValues.FirstOrDefault());
                }

                return new ApiResponseDto(
                    statusCode,
                    dto.ClientId,
                    responseLines
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending receipt");
                return new ApiResponseDto(500, dto.ClientId, new List<string> { ex.Message });
            }
        }

        private ReceiptSenderRequest BuildReceiptRequest(ReceiptSendDto dto)
        {
            var request = new ReceiptSenderRequest
            {
                clave = dto.Clave,
                fecha = dto.Fecha,
                emisor = new Emisor
                {
                    tipoIdentificacion = dto.EmiTipoIdentificacion,
                    numeroIdentificacion = dto.EmiNumeroIdentificacion
                },
                comprobanteXml = dto.ComprobanteXml
            };

            // Add callback URL if provided
            if (!string.IsNullOrEmpty(dto.CallbackUrl))
            {
                request.callbackUrl = dto.CallbackUrl;
            }

            // Add receptor if both fields are provided
            if (!string.IsNullOrEmpty(dto.RecpTipoIdentificacion) && !string.IsNullOrEmpty(dto.RecpNumeroIdentificacion))
            {
                request.receptor = new Receptor
                {
                    tipoIdentificacion = dto.RecpTipoIdentificacion,
                    numeroIdentificacion = dto.RecpNumeroIdentificacion
                };
            }

            return request;
        }
    }
}
