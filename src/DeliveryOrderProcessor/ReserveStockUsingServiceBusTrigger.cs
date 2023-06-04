using System;
using DeliveryOrderProcessor.Models;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Core;
using System.Net.Http;
using System.Net.Http.Json;

namespace DeliveryOrderProcessor;

public class ReserveStockUsingServiceBusTrigger
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public ReserveStockUsingServiceBusTrigger(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    [FunctionName("ReserveStockUsingServiceBusTrigger")]
    public async Task Run([ServiceBusTrigger("order-reserver-queue", Connection = "ServiceBusConnectionString")]string orderReservationJson, ILogger log)
    {
        try
        {
            log.LogInformation($"Service bus queue trigger with Data: {orderReservationJson}");

            var reserverStock = JsonConvert
                .DeserializeObject<OrderDto>(orderReservationJson);

            if (reserverStock is null)
            {
                log.LogError($"{nameof(reserverStock)} failed on bad input: {orderReservationJson}");
                return;
            }

            var reservationId = await UploadOrderReservationJson(orderReservationJson);

            log.LogInformation($"New order reservation: {reservationId} saved to {_configuration["OrderReservationsContainer"]}");
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to reserve stock: {0}\n" +
                "Exception: {1}", orderReservationJson, ex.Message);

            await SendOrderReservationEmailEvent(orderReservationJson, log);
        }
    }

    private async Task<string> UploadOrderReservationJson(string json)
    {
        var reservationId = Guid.NewGuid().ToString();

        BlobClientOptions blobClientOptions = new()
        {
            Retry = {
                MaxRetries = 3,
                Mode = RetryMode.Exponential,
                Delay = TimeSpan.FromMilliseconds(500),
                MaxDelay = TimeSpan.FromSeconds(5),
                NetworkTimeout = TimeSpan.FromSeconds(30)
            }
        };

        BlobContainerClient containerClient = new(_configuration["BlobStorageConnectionString"],
            _configuration["OrderReservationsContainer"],
            blobClientOptions);

        await containerClient.CreateIfNotExistsAsync();

        BlobClient blobClient = containerClient.GetBlobClient($"{reservationId}.json");

        await blobClient.UploadAsync(BinaryData.FromString(json), overwrite: false);

        return reservationId;
    }

    private async Task SendOrderReservationEmailEvent(string reserveItemsJson, ILogger log)
    {
        EnsureConfigurationInfoAvailable(log);

        EmailDto emailDto = new EmailDto()
        {
            To = _configuration["EmailRecipient"],
            Subject = "Order reservation",
            EmailBody = reserveItemsJson
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, 
            new Uri(_configuration["EmailSender"]))
        {
            Content = JsonContent.Create(emailDto)
        };

        var response = await _httpClient.SendAsync(httpRequestMessage);

        log.LogInformation($"Sent order reservation via email. Status: {response.StatusCode}");

    }

    private void EnsureConfigurationInfoAvailable(ILogger log)
    {
        if (string.IsNullOrEmpty(_configuration["EmailSender"]))
        {
            log.LogError("Email sender URL is not set.");
            throw new ArgumentNullException(_configuration["EmailSender"]);
        }

        if (string.IsNullOrEmpty(_configuration["EmailRecipient"]))
        {
            log.LogError("Email recipient is not set.");
            throw new ArgumentNullException(_configuration["EmailRecipient"]);
        }
    }
}
