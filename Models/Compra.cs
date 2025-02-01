using System.ComponentModel.DataAnnotations;

namespace SODERIA_I.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public int clienteId { get; set; }

        [Required]
        public DateTime fecha { get; set; }

        public Cliente cliente { get; set; }
        public int TipoCompraId { get; set; } // Relación con el tipo de compra (bidón/sifón)

        [Required]
        public decimal Cantidad { get; set; } // Cantidad de productos
        public decimal Monto { get; set; }
        public string ClienteNombre { get; set; }
    }
}
