using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using OrderItemsReserver.Models;
using System;
using Azure.Storage.Blobs;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Web.Http;
using Azure.Core;

namespace OrderItemsReserver;

public class ReserveStock
{
    private readonly IConfiguration _configuration;

    public ReserveStock(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [FunctionName("ReserveStock")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        try
        {
            var orderReservationJson = await req.ReadAsStringAsync();
            var reserverStock = JsonConvert
                .DeserializeObject<List<OrderDto>>(orderReservationJson);

            if (reserverStock is null
                || reserverStock.Any())
            {
                log.LogError($"{nameof(reserverStock)} failed on bad input: {orderReservationJson}");

                return new UnprocessableEntityResult();
            }

            var reservationId = await UploadOrderReservationJson(_configuration["OrderReservationsContainer"], 
                orderReservationJson);

            log.LogInformation($"New order reservation: {reservationId}");

            return new OkObjectResult(reservationId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to reserve stock");
            return new ExceptionResult(ex, true);
        }
    }

    public async Task<string> UploadOrderReservationJson(string containerName, string json)
    {
        var reservationId = Guid.NewGuid().ToString();

        BlobClientOptions blobClientOptions = new()
        {
            Retry = {
                MaxRetries = 3,
                Mode = RetryMode.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(10),
                NetworkTimeout = TimeSpan.FromSeconds(100)
            }
        };

        BlobContainerClient containerClient = new(_configuration["AzureWebJobsStorage"], 
            containerName,
            blobClientOptions);

        await containerClient.CreateIfNotExistsAsync();

        BlobClient blobClient = containerClient.GetBlobClient(reservationId);

        await blobClient.UploadAsync(BinaryData.FromString(json), overwrite: false);

        return reservationId;
    }
}
