using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;


namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class DeliveryOrderService : IDeliveryOrderService
{
    private readonly HttpClient _httpClient;

    public DeliveryOrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PostOrderToDeliveryDepartment(Order order)
    {
        DeliveryOrderDto deliveryOrder = new DeliveryOrderDto
        {
            OrderId = order.Id,
            ShippingAddress = new Address(
                order.ShipToAddress.Street,
                order.ShipToAddress.City,
                order.ShipToAddress.State,
                order.ShipToAddress.Country,
                order.ShipToAddress.ZipCode)
        };

        decimal totalPrice = 0;

        foreach (var item in order.OrderItems)
        {
            deliveryOrder.Items.Add(new OrderItemDto()
            {
                CatalogItemId = item.ItemOrdered.CatalogItemId,
                ProductName = item.ItemOrdered.ProductName,
                Quantity = item.Units
            });

            totalPrice += item.UnitPrice * item.Units;
        }

        deliveryOrder.FinalPrice = totalPrice;

        await SendDeliveryOrderToDeliveryOrderProcessor(deliveryOrder);
    }

    private async Task SendDeliveryOrderToDeliveryOrderProcessor(DeliveryOrderDto deliveryOrder)
    {
        var httpRequestMessage
            = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress)
            {
                Content = JsonContent.Create(deliveryOrder)
            };

        HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);

        if (!response.IsSuccessStatusCode)
        {
            throw new DeliveryOrderServiceException(
                $"DeliveryOrderProcessor endpoint call failure: {response.StatusCode}");
        }
    }
}
