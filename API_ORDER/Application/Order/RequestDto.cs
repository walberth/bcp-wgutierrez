namespace API_ORDER.Application.Order
{
    public class RequestDto
    {
        public int IdCliente { get; set; }
        public decimal MontoPago { get; set; }
        public string FormaPago { get; set; }
    }
}
