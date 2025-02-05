using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SODERIA_I.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Zona")]
        public string zona { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Nombre / Apodo")]
        public string nombre { get; set; }

        [Display(Name = "Apellido")]
        public string apellido { get; set; }

        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "El número de teléfono no es válido.")]
        [StringLength(15)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        public string NombreCompleto => $"{nombre} {apellido}";
    }
}
