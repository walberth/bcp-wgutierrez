using API_CONSULTATION.Domain.Consultation;
using MongoDB.Driver;

namespace API_CONSULTATION.Infrastructure
{
    public class ConsultationRepository : IConsultationRepository
    {
        private readonly IMongoCollection<Consultation> _consultationsCollection;

        public ConsultationRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Consultas");
            _consultationsCollection = database.GetCollection<Consultation>("Consultas");
        }

        public async Task Add(Consultation entity)
        {
            await _consultationsCollection.InsertOneAsync(entity);
        }

        public async Task<IEnumerable<Consultation>> GetAll()
        {
            return await _consultationsCollection.Find(_ => true).ToListAsync();
        }
    }
}
