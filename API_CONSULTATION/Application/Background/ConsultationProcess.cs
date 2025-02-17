using API_CONSULTATION.Application.Consultation;
using API_CONSULTATION.CrossCutting;
using API_CONSULTATION.Domain.Consultation;
using Confluent.Kafka;
using MapsterMapper;
using System.Text.Json;

namespace API_CONSULTATION.Application.Background
{
    public class ConsultationProcess : BackgroundService
    {
        private readonly IConsumer<Null, string> _consumer;
        private readonly ILogger<ConsultationProcess> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly TimeSpan _timePeriod = TimeSpan.FromSeconds(Constant.BackgroundSecondsToWait);

        public ConsultationProcess(
            IConsumer<Null, string> consumer,
            ILogger<ConsultationProcess> logger,
            IServiceScopeFactory serviceScopeFactory
        )
        {
            _logger = logger;
            _consumer = consumer;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(_timePeriod);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                Process(stoppingToken);
            }
        }

        private Task Process(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(() =>
            {
                _consumer.Subscribe("consultation-topic");

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);

                        if (result != null)
                        {
                            var consultation = JsonSerializer.Deserialize<ConsultationDto>(result.Message.Value);
                            _logger.LogInformation($"Consumed message: {result.Message.Value}");

                            if (result != null)
                            {
                                ProcessMessageAsync(consultation);
                            }
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    _logger.LogError($"Consumed message throws an error: {ex.Message}");
                    _consumer.Close();
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError($"Consumption error: {ex.Error.Reason}");
                    _consumer.Close();
                }
                finally
                {
                    _logger.LogInformation($"Consumption cllose");
                    _consumer.Close();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private async Task ProcessMessageAsync(ConsultationDto consultation)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var consultationRepository = scope.ServiceProvider.GetRequiredService<IConsultationRepository>();

            var entity = mapper.Map<Domain.Consultation.Consultation>(consultation);
            await consultationRepository.Add(entity);
        }
    }
}
