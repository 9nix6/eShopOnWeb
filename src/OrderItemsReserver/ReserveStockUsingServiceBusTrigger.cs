using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OrderItemsReserver.Models;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Identity;

namespace OrderItemsReserver;

public class ReserveStockUsingServiceBusTrigger
{
    private readonly IConfiguration _configuration;

    public ReserveStockUsingServiceBusTrigger(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    [FunctionName("ReserveStockUsingServiceBusTrigger")]
    public async Task Run([ServiceBusTrigger("order-reserver-queue", Connection = "ServiceBusConnectionString")] string orderReservationJson, ILogger log)
    {
        log.LogInformation($"Service bus queue trigger with Data: {orderReservationJson}");

        var reserverStock = JsonConvert
            .DeserializeObject<OrderDto>(orderReservationJson);

        if (reserverStock is null)
        {
            log.LogError($"{nameof(reserverStock)} failed on bad input: {orderReservationJson}");
            return;
        }

        await Task.FromResult(Task.CompletedTask);
        //try
        //{
        //    var reservationId = await UploadOrderReservationJson(_configuration["OrderReservationsContainer"],
        //        orderReservationJson);

        //    log.LogInformation($"New order reservation: {reservationId}");
        //}
        //catch (Exception ex)
        //{
        //    log.LogError(ex, "Failed to reserve stock: {0}", orderReservationJson);

        //    await SendOrderReservationEvent(orderReservationJson);
        //}
    }

    //public async Task<string> UploadOrderReservationJson(string containerName, string json)
    //{
    //    var reservationId = Guid.NewGuid().ToString();

    //    BlobClientOptions blobClientOptions = new()
    //    {
    //        Retry = {
    //            MaxRetries = 3,
    //            Mode = RetryMode.Exponential,
    //            Delay = TimeSpan.FromSeconds(2),
    //            MaxDelay = TimeSpan.FromSeconds(10),
    //            NetworkTimeout = TimeSpan.FromSeconds(100)
    //        }
    //    };

    //    BlobContainerClient containerClient = new(_configuration["AzureWebJobsStorage"],
    //        containerName,
    //        blobClientOptions);

    //    await containerClient.CreateIfNotExistsAsync();

    //    BlobClient blobClient = containerClient.GetBlobClient(reservationId);

    //    await blobClient.UploadAsync(BinaryData.FromString(json), overwrite: false);

    //    return reservationId;
    //}

    //private async Task SendOrderReservationEvent(string reserveItemsJson)
    //{
    //    if (string.IsNullOrEmpty(_configuration["EventHubTopicEnpoint"]))
    //    {
    //        throw new InvalidOperationException("Event Grid endpoint is not set.");
    //    }

    //    EventGridPublisherClient client = new EventGridPublisherClient(
    //        new Uri(_configuration["EventHubTopicEnpoint"]!),
    //        new DefaultAzureCredential());

    //    await client.SendEventAsync(new EventGridEvent(
    //            "ReserveStockRequest",
    //            "OrderReservationEmail",
    //            "1.0",
    //            reserveItemsJson));
    //}

}
