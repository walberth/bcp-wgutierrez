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

        public async Task<string> Add(Consultation entity)
        {
            await _consultationsCollection.InsertOneAsync(entity);
            return entity.IdConsulta;
        }

        public async Task<IEnumerable<Consultation>> GetAll()
        {
            return await _consultationsCollection.Find(_ => true).ToListAsync();
        }
    }
}
