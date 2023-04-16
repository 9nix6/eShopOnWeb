using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class StockReservationService : IStockReservationService
{
    private readonly HttpClient _httpClient;

    public StockReservationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

        await SendRequestToOrderItemsReserverService(reserveItems);
    }

    private async Task SendRequestToOrderItemsReserverService(List<OrderItemDto> reserveItems)
    {
        var httpRequestMessage
            = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress)
            {
                Content = JsonContent.Create(reserveItems)
            };

        HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

        if (!response.IsSuccessStatusCode)
        {
            throw new StockReservationServiceException(
                $"OrderItemsReserver endpoint call failure: {response.StatusCode}");
        }
    }
}
