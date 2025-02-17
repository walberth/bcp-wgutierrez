using API_ORDER.Domain.Order;
using API_ORDER.Infrastructure.Context;

namespace API_ORDER.Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        protected readonly DatabaseContext _context;

        public OrderRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<int> Add(Order entity)
        {
            await _context.Order.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }
    }
}
