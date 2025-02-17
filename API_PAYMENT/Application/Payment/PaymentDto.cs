using API_PAYMENT.Application.Enums;

namespace API_PAYMENT.Application.Payment
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
