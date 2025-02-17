using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_PAYMENT.Domain.Payment
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdPago { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FechaPago { get; set; }

        public int IdCliente { get; set; }

        public int IdPedido { get; set; }

        public int FormaPago { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal MontoPago { get; set; }
    }
}
