using API_ORDER.Application.Enums;

namespace API_ORDER.Application.Order
{
    public class RequestDto
    {
        public int IdCliente { get; set; }
        public decimal MontoPago { get; set; }
        public FormaPagoEnum FormaPago { get; set; }
    }
}
