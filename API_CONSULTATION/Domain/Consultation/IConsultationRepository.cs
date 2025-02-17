namespace API_CONSULTATION.Domain.Consultation
{
    public interface IConsultationRepository
    {
        Task Add(Consultation entity);

        Task<IEnumerable<Consultation>> GetAll();
    }
}
