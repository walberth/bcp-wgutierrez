using API_ORDER.Domain;
using Confluent.Kafka;
using MapsterMapper;
using System.Text.Json;

namespace API_ORDER.Application.Order
{
    public class OrderHandler
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProducer<Null, string> _producer;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderHandler(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<OrderHandler> logger,
            IProducer<Null, string> producer,
            IHttpClientFactory httpClientFactory)
        {
            _mapper = mapper;
            _logger = logger;
            _producer = producer;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task Process(RequestDto information)
        {
            var order = _mapper.Map<Domain.Order.Order>(information);
            order.FechaPedido = DateTime.Now;

            var client = await _unitOfWork.ClientRepository.Get(information.IdCliente);

            using var transaction = _unitOfWork.BeginTransaction();
            try
            {
                await _unitOfWork.OrderRepository.Add(order, transaction);

                var httpClient = _httpClientFactory.CreateClient("PaymentApiClient");

                var payment = new PaymentDto
                {
                    IdCliente = information.IdCliente,
                    FormaPago = information.FormaPago,
                    IdPedido = order.IdPedido,
                    FechaPago = order.FechaPedido,
                    MontoPago = information.MontoPago
                };

                var paymentAsJson = JsonContent.Create(payment);
                using var response = await httpClient.PostAsync("payment/add", paymentAsJson);

                if (!response.IsSuccessStatusCode)
                {
                    transaction?.Rollback();
                    await Task.CompletedTask;
                }

                payment = await response.Content.ReadFromJsonAsync<PaymentDto>();

                var consultation = new ConsultationDto
                {
                    IdPedido = payment.IdPedido,
                    NombreCliente = client.NombreCliente,
                    IdPago = payment.IdPago,
                    MontoPago = payment.MontoPago
                };

                try
                {
                    var consultationAsJson = JsonSerializer.Serialize(consultation);
                    var result = await _producer.ProduceAsync("consultation-topic", new Message<Null, string> { Value = consultationAsJson });

                    _logger.LogInformation($"Produced message was send, han had status: {result.Status}");
                }
                catch (ProduceException<Null, string> ex)
                {
                    _logger.LogError($"Delivery failed: {ex.Error.Reason}");
                    transaction?.Rollback();
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Delivery failed: {ex.Message}");
                }
                finally
                {
                    _logger.LogInformation("Delivery finnalizing");
                }

                transaction?.Commit();
            }
            catch (Exception)
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                await _unitOfWork!.EndTransaction();
            }

            await Task.CompletedTask;
        }
    }
}
