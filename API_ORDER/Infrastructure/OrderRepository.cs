using API_ORDER.Domain.Order;
using API_ORDER.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace API_ORDER.Infrastructure
{
    public class OrderRepository : IOrderRepository
    {
        protected readonly DatabaseContext _context;

        public OrderRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<int> Add(Order entity, IDbTransaction transaction)
        {
            if (_context.Database.CurrentTransaction == null || transaction == default)
            {
                await _context.Database.UseTransactionAsync((DbTransaction)transaction);
            }

            await _context.Order.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }
    }
}
