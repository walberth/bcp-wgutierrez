using API_ORDER.Domain.Client;
using API_ORDER.Domain.Order;
using System.Data;

namespace API_ORDER.Domain
{
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository ClientRepository { get; }
        IOrderRepository OrderRepository { get; }

        IDbTransaction BeginTransaction();
        Task EndTransaction();
    }
}
