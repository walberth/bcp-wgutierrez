using API_ORDER.Domain.Order;
using MapsterMapper;

namespace API_ORDER.Application.Order
{
    public class OrderHandler
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;

        public OrderHandler(
            IMapper mapper,
            IOrderRepository orderRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
        }

        public async Task Process(RequestDto information)
        {
            var order = _mapper.Map<Domain.Order.Order>(information);
            order.FechaPedido = DateTime.Now;

            await _orderRepository.Add(order);

            await Task.CompletedTask;
        }
    }
}
