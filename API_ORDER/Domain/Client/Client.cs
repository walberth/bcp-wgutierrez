using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_ORDER.Domain.Client
{
    [Table("Cliente")]
    public class Client
    {
        [Key]
        [Column("IdCliente")]
        public int IdCliente { get; set; }

        [Column("NombreCliente")]
        public string NombreCliente { get; set; }

        [NotMapped]
        public Order.Order Order { get; set; }
    }
}
