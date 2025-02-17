namespace API_CONSULTATION.Domain.Consultation
{
    public interface IConsultationRepository
    {
        Task<string> Add(Consultation entity);

        Task<IEnumerable<Consultation>> GetAll();
    }
}
