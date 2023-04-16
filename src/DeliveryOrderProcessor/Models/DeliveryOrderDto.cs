using System;
using System.Collections.Generic;

namespace DeliveryOrderProcessor.Models;

public class DeliveryOrderDto
{
    public int OrderId { get; set; }
    public ShippingAddress ShippingAddress { get; set; }
    public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal FinalPrice { get; set; }

    // Used by CosmosDb
    public string id { get; init; } = Guid.NewGuid().ToString();
}
