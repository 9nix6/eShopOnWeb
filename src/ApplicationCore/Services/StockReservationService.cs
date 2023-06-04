using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using System;
using Microsoft.Extensions.Configuration;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class StockReservationService : IStockReservationService
{
    private readonly IConfiguration _configuration;

    public StockReservationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ReserverStock(Order order)
    {
        List<OrderItemDto> reserveItems = new List<OrderItemDto>();
        foreach (var item in order.OrderItems)
        {
            reserveItems.Add(new OrderItemDto
            {
                CatalogItemId = item.ItemOrdered.CatalogItemId,
                ProductName = item.ItemOrdered.ProductName,
                Quantity = item.Units
            });
        }

        await SendOrderReservationMessage(reserveItems);
    }

    private async Task SendOrderReservationMessage(List<OrderItemDto> reserveItems)
    {
        ArgumentException.ThrowIfNullOrEmpty(_configuration["ServiceBusConnectionString"]);
        ArgumentException.ThrowIfNullOrEmpty(_configuration["ServiceBusQueueName"]);

        var clientOptions = new ServiceBusClientOptions()
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        ServiceBusClient client = new ServiceBusClient(_configuration["ServiceBusConnectionString"], clientOptions);
        ServiceBusSender sender = client.CreateSender(_configuration["ServiceBusQueueName"]);

        try
        {
            using ServiceBusMessageBatch messageBatch = await BuildMessage(reserveItems, sender);

            await sender.SendMessagesAsync(messageBatch);
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }

    private static async Task<ServiceBusMessageBatch> BuildMessage(List<OrderItemDto> reserveItems, ServiceBusSender sender)
    {
        ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

        foreach (var item in reserveItems)
        {
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(JsonSerializer.Serialize(item))))
            {
                throw new Exception($"The message is too large to fit in the batch.");
            }
        }

        return messageBatch;
    }
}
