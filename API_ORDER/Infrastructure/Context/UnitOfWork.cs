using API_ORDER.Domain;
using API_ORDER.Domain.Client;
using API_ORDER.Domain.Order;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace API_ORDER.Infrastructure.Context
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;
        private readonly IDbConnection _dbConnection;

        public IClientRepository ClientRepository { get; }
        public IOrderRepository OrderRepository { get; }

        public UnitOfWork(
            DatabaseContext context,
            IOrderRepository orderRepository,
            IClientRepository clientRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbConnection = context.Database.GetDbConnection();

            OrderRepository = orderRepository;
            ClientRepository = clientRepository;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

        public IDbTransaction BeginTransaction()
        {
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }

            return _dbConnection.BeginTransaction();
        }

        public async Task EndTransaction()
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await _context.Database.CurrentTransaction.DisposeAsync();
            }
        }
    }
}
