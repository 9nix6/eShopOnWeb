using System;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;
public class StockReservationServiceException : Exception
{
    public StockReservationServiceException(string message) : base(message)
    {

    }
}
