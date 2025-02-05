using System.ComponentModel.DataAnnotations;

namespace SODERIA_I.Models
{
    public class Compra
    {
        public int Id { get; set; }
        public int clienteId { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        public DateTime fecha { get; set; }

        [Display(Name = "Cliente")]
        public Cliente cliente { get; set; }

        [Display(Name = "Tipo de compra")]
        public int TipoCompraId { get; set; } // Relación con el tipo de compra (bidón/sifón)

        [Required]
        public decimal Cantidad { get; set; } // Cantidad de productos
        public decimal Monto { get; set; }
        public string ClienteNombre { get; set; }
    }
}
