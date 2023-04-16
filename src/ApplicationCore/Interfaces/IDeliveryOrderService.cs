using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;
public interface IDeliveryOrderService
{
    Task PostOrderToDeliveryDepartment(Order order);
}