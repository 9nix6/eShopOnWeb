using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DeliveryOrderProcessor.Models;
using System.Linq;
using Microsoft.Azure.Cosmos;
using System.Configuration;
using System;
using System.Text.Json;

namespace DeliveryOrderProcessor
{
    public class AddOrderDelivery
    {
        private IConfiguration _configuration;  
        private readonly CosmosClient _cosmosClient;

        public AddOrderDelivery(CosmosClient cosmosClient, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
        }

        [FunctionName("AddOrderDelivery")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var deliveryOrderJson = await req.ReadAsStringAsync();
            var deliveryOrder = JsonSerializer
                .Deserialize<DeliveryOrderDto>(deliveryOrderJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true
                });

            if (deliveryOrder is null 
                || !deliveryOrder.Items.Any() 
                || deliveryOrder.FinalPrice <= 0)
            {
                log.LogError($"{nameof(AddOrderDelivery)} failed on bad input: {deliveryOrderJson}");

                return new UnprocessableEntityResult();
            }

            try
            {
                var container = _cosmosClient.GetContainer("delivery", "order");
                var result = await container.CreateItemAsync(deliveryOrder);

                return new OkObjectResult(result.Resource.id);
            }
            catch (CosmosException cosmosException)
            {
                log.LogError("Creating item failed with error {0}", cosmosException.ToString());
                return new BadRequestObjectResult($"Failed to create item. Cosmos Status Code {cosmosException.StatusCode}, Sub Status Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }
    }
}
