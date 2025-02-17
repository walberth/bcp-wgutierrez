using API_ORDER.Domain.Client;
using API_ORDER.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace API_ORDER.Infrastructure
{
    public class ClientRepository : IClientRepository
    {
        protected readonly DatabaseContext _context;

        public ClientRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetAll()
        {
            return await _context.Client.ToListAsync();
        }
    }
}
