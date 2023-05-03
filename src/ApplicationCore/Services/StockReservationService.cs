using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using System;
using Azure.Messaging.EventGrid;
using Azure.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Core.Serialization;
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

        await SendOrderReservationEvent(reserveItems);
    }

    private async Task SendOrderReservationEvent(List<OrderItemDto> reserveItems)
    {
        const string EventGridEndpoint = "https://artur-eshop-topic.westeurope-1.eventgrid.azure.net/api/events";

        EventGridPublisherClient client = new EventGridPublisherClient(
            new Uri(EventGridEndpoint),//_configuration["EventHubTopicEnpoint"]),
            new DefaultAzureCredential());

        List<EventGridEvent> eventsList = new List<EventGridEvent>();
        foreach (var item in reserveItems) 
        {
            eventsList.Add(new EventGridEvent(
                "ReserveStockRequest",
                "OrderReservation",
                "1.0",
                item));
        }

        // Send the events
        await client.SendEventsAsync(eventsList);
    }
}
