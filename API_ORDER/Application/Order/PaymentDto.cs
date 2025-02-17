using API_ORDER.Application.Enums;

namespace API_ORDER.Application.Order
{
    public class PaymentDto
    {
        public string IdPago { get; set; }
        public DateTime FechaPago { get; set; }
        public int IdCliente { get; set; }
        public int IdPedido { get; set; }
        public FormaPagoEnum FormaPago { get; set; }
        public decimal MontoPago { get; set; }
    }
}
