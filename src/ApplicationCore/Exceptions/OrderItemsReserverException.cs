using System;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;
public class OrderItemsReserverException : Exception
{
    public OrderItemsReserverException(string message) : base(message)
    {

    }
}
