namespace Microsoft.eShopWeb.ApplicationCore.Entities;
internal class OrderItemDto
{
    public int CatalogItemId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
}
