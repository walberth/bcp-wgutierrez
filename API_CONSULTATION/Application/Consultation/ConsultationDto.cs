using API_CONSULTATION.Application.Enums;

namespace API_CONSULTATION.Application.Consultation
{
    public class ConsultationDto
    {
        public string IdConsulta { get; set; }
        public int IdPedido { get; set; }
        public string NombreCliente { get; set; }
        public string IdPago { get; set; }
        public decimal MontoPago { get; set; }
        public FormaPagoEnum FormaPago { get; set; }
        public string? FormaPagoValue { get; set; }
    }
}
