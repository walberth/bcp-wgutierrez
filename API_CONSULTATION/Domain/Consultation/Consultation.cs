using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_CONSULTATION.Domain.Consultation
{
    public class Consultation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdConsulta { get; set; }
        public int IdPedido { get; set; }
        public string NombreCliente { get; set; }
        public string IdPago { get; set; }
        public int FormaPago { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MontoPago { get; set; }
    }
}
