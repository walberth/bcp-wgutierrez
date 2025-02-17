namespace API_PAYMENT.Domain.Payment
{
    public interface IPaymentRepository
    {
        Task<string> Add(Payment entity);

        Task<IEnumerable<Payment>> GetAll();
    }
}
