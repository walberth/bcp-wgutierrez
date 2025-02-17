namespace API_ORDER.Domain.Client
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAll();
        Task<Client> Get(int idCliente);
    }
}
