using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SODERIA_I.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        public string zona { get; set; }

        [Required]
        [StringLength(20)]
        public string nombre { get; set; }

        [Required]
        public string apellido { get; set; }

        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "El número de teléfono no es válido.")]
        [StringLength(15)]
        public string Telefono { get; set; }

        public string NombreCompleto => $"{nombre} {apellido}";
    }
}
