using System.Collections.Generic;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Entities;
internal class DeliveryOrderDto
{
    public int OrderId { get; set; }
    public Address ShippingAddress { get; set; }
    public IList<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    public decimal FinalPrice { get; set; }
}
