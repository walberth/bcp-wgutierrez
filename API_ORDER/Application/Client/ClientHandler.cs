using API_ORDER.Domain.Client;
using MapsterMapper;

namespace API_ORDER.Application.Client
{
    public class ClientHandler
    {
        private readonly IMapper _mapper;
        private readonly IClientRepository _clientRepository;

        public ClientHandler(
            IMapper mapper,
            IClientRepository clientRepository)
        {
            _mapper = mapper;
            _clientRepository = clientRepository;
        }

        public async Task<IEnumerable<ClientDto>> GetAll()
        {
            var clientsInDb = await _clientRepository.GetAll();

            return _mapper.Map<IEnumerable<ClientDto>>(clientsInDb);
        }
    }
}
