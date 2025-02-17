using API_PAYMENT.Domain.Payment;
using MongoDB.Driver;

namespace API_PAYMENT.Infrastructure
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payment> _paymentsCollection;

        public PaymentRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Pagos");
            _paymentsCollection = database.GetCollection<Payment>("Pagos");
        }

        public async Task<string> Add(Payment entity)
        {
            await _paymentsCollection.InsertOneAsync(entity);
            return entity.IdPago;
        }

        public async Task<IEnumerable<Payment>> GetAll()
        {
            return await _paymentsCollection.Find(_ => true).ToListAsync();
        }
    }
}
