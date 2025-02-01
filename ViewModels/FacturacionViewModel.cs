using SODERIA_I.Models;
using System.ComponentModel.DataAnnotations;

namespace SODERIA_I.ViewModels
{
    public class FacturacionViewModel
    {
        public List<CompraPorTipoViewModel> ComprasPorTipo { get; set; }
    }
    public class CompraPorTipoViewModel
    {
        public int TipoCompraId { get; set; }
        public string TipoCompraNombre { get; set; }
        public decimal CantidadTotal { get; set; }
    }
}
