using System;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;
public class DeliveryOrderServiceException : Exception
{
    public DeliveryOrderServiceException(string message) : base(message)
    {

    }
}
