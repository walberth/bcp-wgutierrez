using API_PAYMENT.Domain.Payment;
using MapsterMapper;

namespace API_PAYMENT.Application.Payment
{
    public class PaymentHandler
    {
        private readonly IMapper _mapper;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentHandler(
            IMapper mapper,
            IPaymentRepository paymentRepository)
        {
            _mapper = mapper;
            _paymentRepository = paymentRepository;
        }

        public async Task Add(PaymentDto information)
        {
            var payment = _mapper.Map<Domain.Payment.Payment>(information);

            await _paymentRepository.Add(payment);

            await Task.CompletedTask;
        }

        public async Task<IEnumerable<PaymentDto>> GetAll()
        {
            var payments = await _paymentRepository.GetAll();

            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }
    }
}
