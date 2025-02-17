using API_CONSULTATION.Application.Consultation;
using API_CONSULTATION.Domain.Consultation;
using Confluent.Kafka;
using MapsterMapper;
using System.Text.Json;

namespace API_CONSULTATION.Application.Background
{
    public class ConsultationProcess : BackgroundService
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsultationProcess> _logger;

        public ConsultationProcess(
            IConsumer<Null, string> consumer,
            IServiceProvider serviceProvider,
            ILogger<ConsultationProcess> logger
        )
        {
            _logger = logger;
            _consumer = consumer;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _consumer.Subscribe("consultation-topic");

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);
                        var consultation = JsonSerializer.Deserialize<ConsultationDto>(result.Message.Value);

                        if (consultation != null)
                        {
                            _logger.LogInformation($"Consumed message: {result.Message.Value}");

                            using var scope = _serviceProvider.CreateScope();

                            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                            var consultationRepository = scope.ServiceProvider.GetRequiredService<IConsultationRepository>();

                            var entity = mapper.Map<Domain.Consultation.Consultation>(consultation);

                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    _logger.LogError($"Consumed message throws an error: {ex.Message}");
                    _consumer.Close();
                }
            }, stoppingToken);
        }
    }
}
