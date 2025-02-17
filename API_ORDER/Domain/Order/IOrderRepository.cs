using System.Data;

namespace API_ORDER.Domain.Order
{
    public interface IOrderRepository
    {
        Task<int> Add(Order entity, IDbTransaction transaction);
    }
}
