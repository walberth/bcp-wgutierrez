using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_ORDER.Domain.Order
{
    [Table("Pedido")]
    public class Order
    {
        [Key]
        [Column("IdPedido")]
        public int IdPedido { get; set; }

        [Column("FechaPedido")]
        public DateTime FechaPedido { get; set; }

        [Column("IdCliente")]
        public int IdCliente { get; set; }

        [Column("MontoPedido")]
        public decimal MontoPedido { get; set; }

        [NotMapped]
        public Client.Client Client { get; set; }
    }
}
