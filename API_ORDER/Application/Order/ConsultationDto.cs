using API_ORDER.Application.Enums;

namespace API_ORDER.Application.Order
{
    public class ConsultationDto
    {
        public int IdPedido { get; set; }
        public string NombreCliente { get; set; }
        public string IdPago { get; set; }
        public decimal MontoPago { get; set; }
        public FormaPagoEnum FormaPago { get; set; }
    }
}
